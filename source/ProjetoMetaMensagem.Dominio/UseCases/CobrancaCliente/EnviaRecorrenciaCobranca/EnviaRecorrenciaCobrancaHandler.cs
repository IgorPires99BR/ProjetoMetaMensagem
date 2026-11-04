using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.EnviaRecorrenciaCobranca
{
    // Botao da tela de Cobrancas de Clientes: reenvia ao cliente a mensagem de uma cobranca em
    // aberto, com o template marcado em Momentos de disparo (TemplateGatilho.RecorrenciaManual).
    // Nao abre outra cobranca: o valor em aberto contaria duas vezes.
    public class EnviaRecorrenciaCobrancaHandler : IRequestHandler<EnviaRecorrenciaCobrancaCommand, Response<EnviaRecorrenciaCobrancaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPreparadorCobrancaPix _preparadorPix;
        private readonly IMediator _mediator;
        private readonly ILogger<EnviaRecorrenciaCobrancaHandler> _logger;

        public EnviaRecorrenciaCobrancaHandler(IUnitOfWork unitOfWork, IPreparadorCobrancaPix preparadorPix, IMediator mediator,
                                               ILogger<EnviaRecorrenciaCobrancaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _preparadorPix = preparadorPix;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Response<EnviaRecorrenciaCobrancaResult>> Handle(EnviaRecorrenciaCobrancaCommand command)
        {
            var response = new Response<EnviaRecorrenciaCobrancaResult>();

            var validateResult = new EnviaRecorrenciaCobrancaValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorId(command.CobrancaClienteId, command.EmpresaIdSolicitante);
                if (cobranca == null)
                {
                    response.AddErro("Cobrança não encontrada.", 404);
                    return response;
                }

                if (cobranca.Status != StatusCobrancaCliente.Pendente)
                {
                    response.AddErro("Esta cobrança não está mais em aberto.");
                    return response;
                }

                var gatilhos = (await _unitOfWork.Template.ObterGatilhosAtivos(cobranca.EmpresaId, TemplateGatilho.RecorrenciaManual)).ToList();
                if (gatilhos.Count == 0)
                {
                    response.AddErro("Nenhum modelo de recorrência configurado. Em Templates, abra Momentos de disparo do modelo e ligue o botão de recorrência.");
                    return response;
                }
                if (gatilhos.Count > 1)
                {
                    response.AddErro("Há mais de um modelo com o botão de recorrência ligado. Deixe ligado só o que deve ser enviado.");
                    return response;
                }

                var gatilho = gatilhos[0];
                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(gatilho.TemplateId, cobranca.EmpresaId);
                if (template == null || template.Status == null || !template.Status.StartsWith("APPROVED", StringComparison.OrdinalIgnoreCase))
                {
                    response.AddErro("O modelo de recorrência ainda não foi aprovado pela Meta.");
                    return response;
                }

                if (template.GeraCobranca)
                {
                    response.AddErro("O modelo de recorrência está com \"Gera cobrança\" ligado, o que abriria outra cobrança. Desligue essa opção no modelo.");
                    return response;
                }

                var contato = (await _unitOfWork.Contato.ObterPorIds(cobranca.EmpresaId, new[] { cobranca.ContatoId })).FirstOrDefault();
                if (contato == null)
                {
                    response.AddErro("O contato desta cobrança não existe mais.");
                    return response;
                }

                var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(cobranca.EmpresaId)).ToDictionary(p => p.Id);
                var agora = HorarioBrasilia.Agora();
                List<string> Valores() => gatilho.Variaveis
                    .Select(v => ResolvedorDeVariaveis.ResolverDaCobranca(v, contato, cobranca, parametros, agora))
                    .ToList();

                // Barra antes de mexer no Pix: um botao de link fixo ja mandou cliente para o checkout errado.
                var temBotao = PixNoTemplate.TemBotaoUrlDinamico(template);
                if (!temBotao && !Valores().Any(v => v == ResolvedorDeVariaveis.MarcadorLinkPagamento || v == ResolvedorDeVariaveis.MarcadorPixCopiaECola))
                {
                    response.AddErro("O modelo de recorrência não leva o Pix: precisa do botão \"Pagar com Pix\" ou de uma variável de Pix.");
                    return response;
                }

                var pixAnterior = cobranca.PixIdExterno;
                var preparada = await _preparadorPix.GarantirPixAsync(cobranca, contato, agora);
                if (preparada.Erro != null)
                {
                    response.AddErro(preparada.Erro);
                    return response;
                }

                // Depois do Pix: o valor atualizado da mensagem e o que ele acabou de calcular.
                var valores = Valores();
                var erroPix = _preparadorPix.AplicarNasVariaveis(valores, preparada);
                if (erroPix != null)
                {
                    response.AddErro(erroPix);
                    return response;
                }

                var contatoId = contato.Id.ToString();
                var envio = new EnviarMensagemTemplateMetaLoteCommand
                {
                    IdEmpresa = cobranca.EmpresaId,
                    EmpresaId = cobranca.EmpresaId,
                    TemplateId = template.Id,
                    NomeTemplate = template.NomeTemplate,
                    Idioma = template.Idioma ?? "pt_BR",
                    Telefones = new List<string> { contato.Telefone },
                    ContatosIds = new List<string> { contatoId },
                    ParametrosBody = valores,
                    Origem = OrigemDisparo.GatilhoTemplate
                };
                if (valores.Count > 0) envio.ParametrosBodyPorContato[contatoId] = valores;
                if (temBotao) envio.ParametrosButtonPorContato[contatoId] = new List<string> { preparada.Cobranca.Txid! };
                if (PixNoTemplate.TemCabecalhoImagem(template)) envio.HeaderMediaUrlPorContato[contatoId] = preparada.UrlQrCode!;

                var resultado = await _mediator.Send(envio);
                if (resultado is null || resultado.HasValidations)
                {
                    response.AddErro(resultado is null ? "Não foi possível enviar a mensagem." : string.Join(" ", resultado.Erros));
                    return response;
                }

                if (resultado.Value.TotalSucesso == 0)
                {
                    var motivo = string.Join(" | ", resultado.Value.RelatorioErros.Values.Distinct());
                    response.AddErro(string.IsNullOrWhiteSpace(motivo) ? "A Meta não aceitou o envio da mensagem." : motivo);
                    return response;
                }

                response.AddValue(new EnviaRecorrenciaCobrancaResult
                {
                    CobrancaClienteId = cobranca.Id,
                    TotalEnviados = resultado.Value.TotalSucesso,
                    TotalFalhas = resultado.Value.TotalFalha,
                    PixRenovado = pixAnterior != cobranca.PixIdExterno,
                    ValorCobrado = cobranca.ValorCobrado
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(EnviaRecorrenciaCobrancaHandler));
            }

            return response;
        }
    }
}
