using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Executa os momentos de disparo configurados na tela de Templates (TemplateGatilho).
    // Chamado pelo HangFire: o de cadastro enfileirado pelo controller de Contato (o cadastro nao
    // espera a Meta responder), o de atraso como job recorrente diario.
    public interface IDisparoPorGatilho
    {
        Task DispararCadastroContatoAsync(Guid empresaId, Guid contatoId);
        Task ProcessarAtrasosAsync();
    }

    public class DisparoPorGatilho : IDisparoPorGatilho
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<DisparoPorGatilho> _logger;

        public DisparoPorGatilho(IUnitOfWork unitOfWork, IMediator mediator, ILogger<DisparoPorGatilho> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task DispararCadastroContatoAsync(Guid empresaId, Guid contatoId)
        {
            var gatilhos = (await _unitOfWork.Template.ObterGatilhosAtivos(empresaId, TemplateGatilho.CadastroContato)).ToList();
            if (gatilhos.Count == 0) return;

            var contato = (await _unitOfWork.Contato.ObterPorIds(empresaId, new[] { contatoId })).FirstOrDefault();
            if (contato == null)
            {
                _logger.LogWarning("Gatilho de cadastro: contato {ContatoId} nao encontrado na empresa {EmpresaId}.", contatoId, empresaId);
                return;
            }

            foreach (var gatilho in gatilhos)
            {
                await Disparar(gatilho, new List<Contato> { contato });
            }
        }

        public async Task ProcessarAtrasosAsync()
        {
            var pendentes = (await _unitOfWork.Template.ObterCobrancasEmAtrasoSemAviso(DateTime.Now)).ToList();
            if (pendentes.Count == 0) return;

            _logger.LogInformation("Gatilho de atraso: {Total} cobranca(s) em atraso para avisar.", pendentes.Count);

            foreach (var porEmpresa in pendentes.GroupBy(p => p.EmpresaId))
            {
                var gatilhos = (await _unitOfWork.Template.ObterGatilhosAtivos(porEmpresa.Key, TemplateGatilho.AtrasoPagamento))
                    .ToDictionary(g => g.Id);

                foreach (var porGatilho in porEmpresa.GroupBy(p => p.GatilhoId))
                {
                    if (gatilhos.TryGetValue(porGatilho.Key, out var gatilho))
                        await AvisarAtrasos(gatilho, porGatilho.ToList());
                }
            }
        }

        private async Task AvisarAtrasos(TemplateGatilho gatilho, List<CobrancaEmAtraso> cobrancas)
        {
            // Mesma cobranca nao se repete por gatilho, mas um contato com duas cobrancas em
            // atraso recebe um aviso so.
            var contatoIds = cobrancas.Select(p => p.ContatoId).Distinct().ToList();
            var contatos = new List<Contato>();
            foreach (var lote in contatoIds.Chunk(1000))
                contatos.AddRange(await _unitOfWork.Contato.ObterPorIds(gatilho.EmpresaId, lote));

            var enviados = await Disparar(gatilho, contatos);

            // So marca como avisado quem a Meta aceitou: falha (ex: credencial fora) tenta de
            // novo na proxima passada.
            foreach (var item in cobrancas.Where(p => enviados.Contains(p.ContatoId)))
                await _unitOfWork.Template.RegistrarEnvioGatilho(item.GatilhoId, item.CobrancaClienteId);
        }

        // Devolve os contatos cujo envio a Meta aceitou.
        private async Task<HashSet<Guid>> Disparar(TemplateGatilho gatilho, List<Contato> contatos)
        {
            var enviados = new HashSet<Guid>();
            if (contatos.Count == 0) return enviados;

            var template = await _unitOfWork.Template.ObterPorIdEEmpresa(gatilho.TemplateId, gatilho.EmpresaId);
            if (template == null)
            {
                _logger.LogWarning("Gatilho {GatilhoId}: template {TemplateId} nao encontrado.", gatilho.Id, gatilho.TemplateId);
                return enviados;
            }

            // Template ainda em analise ou recusado: a Meta recusaria todos os envios.
            if (template.Status == null || !template.Status.StartsWith("APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Gatilho {GatilhoId}: template {Template} com status {Status}, nada enviado.",
                    gatilho.Id, template.NomeTemplate, template.Status);
                return enviados;
            }

            var comando = new EnviarMensagemTemplateMetaLoteCommand
            {
                IdEmpresa = gatilho.EmpresaId,
                EmpresaId = gatilho.EmpresaId,
                TemplateId = template.Id,
                NomeTemplate = template.NomeTemplate,
                Idioma = template.Idioma ?? "pt_BR",
                Telefones = contatos.Select(c => c.Telefone).ToList(),
                ContatosIds = contatos.Select(c => c.Id.ToString()).ToList(),
                // O handler do lote resolve as variaveis por contato a partir daqui.
                Variaveis = gatilho.Variaveis,
                Origem = OrigemDisparo.GatilhoTemplate
            };

            var resultado = await _mediator.Send(comando);
            if (resultado is null || resultado.HasValidations)
            {
                var motivo = resultado is null ? "sem resposta" : string.Join("; ", resultado.Erros);
                _logger.LogError("Gatilho {GatilhoId} ({Tipo}): falha ao disparar o template {Template} -- {Motivo}",
                    gatilho.Id, gatilho.Tipo, template.NomeTemplate, motivo);
                return enviados;
            }

            foreach (var erro in resultado.Value.RelatorioErros)
            {
                _logger.LogWarning("Gatilho {GatilhoId} ({Tipo}): falha ao enviar para {Telefone} -- {Motivo}",
                    gatilho.Id, gatilho.Tipo, erro.Key, erro.Value);
            }

            foreach (var contato in contatos)
            {
                if (resultado.Value.RelatorioDisparos.TryGetValue(contato.Telefone, out var ok) && ok)
                    enviados.Add(contato.Id);
            }

            _logger.LogInformation("Gatilho {GatilhoId} ({Tipo}): {Sucessos} sucesso(s), {Falhas} falha(s).",
                gatilho.Id, gatilho.Tipo, resultado.Value.TotalSucesso, resultado.Value.TotalFalha);

            return enviados;
        }
    }
}
