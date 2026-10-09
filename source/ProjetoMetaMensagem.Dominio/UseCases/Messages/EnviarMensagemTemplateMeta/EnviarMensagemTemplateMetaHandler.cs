using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Servicos;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaHandler : IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>
    {
        private readonly IMetaService _whatsappService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPreparadorCobrancaPix _preparadorPix;
        private readonly ILogger<EnviarMensagemTemplateMetaHandler> _logger;

        public EnviarMensagemTemplateMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork, IPreparadorCobrancaPix preparadorPix, ILogger<EnviarMensagemTemplateMetaHandler> logger)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
            _preparadorPix = preparadorPix;
            _logger = logger;
        }

        public async Task<Response<EnviarMensagemTemplateMetaResult>> Handle(EnviarMensagemTemplateMetaCommand command)
        {
            var response = new Response<EnviarMensagemTemplateMetaResult>();

            var validator = new EnviarMensagemTemplateMetaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {

                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                var templateEnviado = command.TemplateId.HasValue
                    ? await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId.Value, command.IdEmpresa)
                    : null;

                // Mesma regra do lote: cobranca e Pix nascem antes do envio, porque o Pix vai
                // dentro da mensagem.
                CobrancaPreparada? preparada = null;
                Entidades.Contato? contato = null;
                if (templateEnviado != null && templateEnviado.GeraCobranca)
                {
                    contato = (await _unitOfWork.Contato.ObterPorIds(command.IdEmpresa, new[] { command.ContatoId }))
                        .FirstOrDefault();
                    if (contato != null)
                    {
                        var preparadas = await _preparadorPix.PrepararAsync(command.IdEmpresa, templateEnviado, new[] { contato }, HorarioBrasilia.Agora());
                        preparada = preparadas[contato.Id];
                    }
                }

                var erroPix = preparada?.Erro
                    ?? PixNoTemplate.ErroDoBotaoSemPix(templateEnviado, preparada, command.ParametrosButton?.Any(p => !string.IsNullOrWhiteSpace(p)) ?? false)
                    ?? _preparadorPix.AplicarNasVariaveis(command.ParametrosBody, preparada);
                if (erroPix != null)
                {
                    response.AddErro(erroPix);
                    return response;
                }

                if (preparada != null && preparada.TemPix)
                {
                    if (PixNoTemplate.TemBotaoUrlDinamico(templateEnviado!) && !(command.ParametrosButton?.Any(p => !string.IsNullOrWhiteSpace(p)) ?? false))
                        command.ParametrosButton = new List<string> { preparada.Cobranca.Txid! };
                    if (PixNoTemplate.TemCabecalhoImagem(templateEnviado!) && string.IsNullOrWhiteSpace(command.ParametroHeaderMediaUrl))
                        command.ParametroHeaderMediaUrl = preparada.UrlQrCode;
                }

                // 2. Chamada ao serviço de integração com a Meta
                var respostaMeta = await _whatsappService.EnviarTemplateAsync(command, phoneNumberId, token);

                if (respostaMeta == null || !respostaMeta.Sucesso)
                {
                    if (preparada != null)
                        await _preparadorPix.DescartarAsync(command.IdEmpresa, new[] { preparada });

                    response.AddErro(respostaMeta == null ? "Erro ao Acessar a meta" : $"Falha no disparo da Meta: {respostaMeta.Erro}");
                    return response;
                }

                // 3. Persistencia no historico em caso de sucesso
                // Grava o texto final, ja com as variaveis trocadas: e isso que o chat e o
                // relatorio mostram pro usuario.

                var historico = new HistoricoDisparo
                {
                    EmpresaId = command.IdEmpresa,
                    ContatoId = command.ContatoId,
                    TemplateId = command.TemplateId,
                    TipoDisparo = "Template",
                    WamidMeta = respostaMeta.WamidMeta,
                    Conteudo = TemplateTextoHelper.MontarTextoEnviado(
                        templateEnviado?.Conteudo,
                        command.NomeTemplate,
                        command.ParametrosBody),
                    Origem = command.Origem
                };

                await _unitOfWork.HistoricoDisparo.Incluir(historico);

                // Template marcado GeraCobranca (ex: aviso de cobranca da Sebrecon): abre a
                // CobrancaCliente vinculada a este disparo. Snapshot do Contato no momento do
                // envio -- ver CobrancaClienteFactory.
                if (templateEnviado != null && templateEnviado.GeraCobranca)
                {
                    if (preparada != null)
                    {
                        preparada.Cobranca.HistoricoDisparoId = historico.Id;
                        await _unitOfWork.CobrancaCliente.Incluir(preparada.Cobranca);
                    }
                    else
                    {
                        // Nao devolve erro: a mensagem ja foi enviada de verdade, so nao ha como
                        // rastrear a cobranca sem o cadastro do contato (nao deveria acontecer,
                        // ja que o ContatoId vem do proprio destinatario do disparo).
                        _logger.LogWarning(
                            "GeraCobranca: contato {ContatoId} nao encontrado ao abrir CobrancaCliente do disparo {HistoricoDisparoId}",
                            command.ContatoId, historico.Id);
                    }
                }

                // 4. Montagem do resultado positivo
                var resultado = new EnviarMensagemTemplateMetaResult
                {
                    Sucesso = true,
                    WamidMeta = respostaMeta.WamidMeta
                };

                response.AddValue(resultado);

            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(EnviarMensagemTemplateMetaHandler));
            }

            return response;
        }
    }
}
