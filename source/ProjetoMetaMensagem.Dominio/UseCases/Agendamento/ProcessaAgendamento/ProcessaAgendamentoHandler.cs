using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Chamado pela AgendamentoMensagemTarefa (HangFire, a cada 5 min) via mediator -- toda a
    // logica de negocio do processamento mora aqui, seguindo o mesmo desenho Command/Handler
    // dos demais casos de uso; a tarefa em si so dispara o comando.
    public class ProcessaAgendamentoHandler : IRequestHandler<ProcessaAgendamentoCommand, Response<ProcessaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessaAgendamentoHandler> _logger;

        public ProcessaAgendamentoHandler(IUnitOfWork unitOfWork, IMediator mediator, ILogger<ProcessaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Response<ProcessaAgendamentoResult>> Handle(ProcessaAgendamentoCommand command)
        {
            var response = new Response<ProcessaAgendamentoResult>();

            var validator = new ProcessaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var resultado = new ProcessaAgendamentoResult();

            try
            {
                var pendentes = (await _unitOfWork.Agendamento.ObterPendentes(DateTime.Now)).ToList();

                if (pendentes.Count == 0)
                {
                    _logger.LogInformation("Varredura de agendamentos: nenhum pendente no momento.");
                    response.AddValue(resultado);
                    return response;
                }

                // Agrupado por empresa: uma unica tarefa cuida de todos os clientes, um de
                // cada vez, em vez de precisar de uma chamada por agendamento.
                var porEmpresa = pendentes.GroupBy(a => a.EmpresaId);

                foreach (var grupoEmpresa in porEmpresa)
                {
                    _logger.LogInformation("Empresa {EmpresaId}: {Total} agendamento(s) pendente(s).",
                        grupoEmpresa.Key, grupoEmpresa.Count());

                    foreach (var agendamento in grupoEmpresa)
                    {
                        resultado.TotalAgendamentos++;

                        try
                        {
                            // Reserva por prazo: se outra passada ja pegou este agendamento
                            // (deploy sobreposto, execucao atrasada), pula sem reenviar.
                            var prazoProcessamento = DateTime.Now.AddMinutes(10);
                            if (!await _unitOfWork.Agendamento.ReivindicarAgendamento(agendamento.Id, prazoProcessamento))
                            {
                                _logger.LogInformation("Agendamento {AgendamentoId} ja estava sendo processado por outra execucao; pulando.",
                                    agendamento.Id);
                                continue;
                            }

                            await ProcessarUmAgendamento(agendamento, resultado);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao processar agendamento {AgendamentoId} da empresa {EmpresaId}",
                                agendamento.Id, agendamento.EmpresaId);
                        }
                    }
                }

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ProcessaAgendamentoHandler));
            }

            return response;
        }

        private async Task ProcessarUmAgendamento(Entidades.Agendamento agendamento, ProcessaAgendamentoResult resultado)
        {
            var contatoIds = (await _unitOfWork.Agendamento.ObterContatoIds(agendamento.Id)).Distinct().ToList();

            // Busca em lotes: o IN (@Ids) do ObterPorIds vira um parametro por id, e o SQL
            // Server corta em 2100 -- mesmo cuidado do CampanhaWorker.
            var contatos = new List<Entidades.Contato>();
            foreach (var lote in contatoIds.Chunk(1000))
            {
                contatos.AddRange(await _unitOfWork.Contato.ObterPorIds(agendamento.EmpresaId, lote));
            }

            _logger.LogInformation("Agendamento {AgendamentoId} ({Nome}): {Total} contato(s) para disparo.",
                agendamento.Id, agendamento.Nome, contatos.Count);

            var template = await _unitOfWork.Template.ObterPorIdEEmpresa(agendamento.TemplateId, agendamento.EmpresaId);

            var sucessos = 0;
            var falhas = 0;

            if (template == null)
            {
                _logger.LogWarning("Template {TemplateId} do agendamento {AgendamentoId} nao foi encontrado; nenhuma mensagem sera enviada.",
                    agendamento.TemplateId, agendamento.Id);
                falhas = contatos.Count;
            }
            else if (contatos.Count > 0)
            {
                // Disparo em lote (mesma estrutura usada pela tela de Disparos): um unico
                // envio pra todos os contatos deste agendamento, em vez de uma chamada por
                // contato -- menos round-trips e reaproveita o registro de HistoricoDisparo
                // que o handler do lote ja faz.
                var comandoLote = new EnviarMensagemTemplateMetaLoteCommand
                {
                    IdEmpresa = agendamento.EmpresaId,
                    EmpresaId = agendamento.EmpresaId,
                    TemplateId = agendamento.TemplateId,
                    NomeTemplate = template.NomeTemplate,
                    Idioma = template.Idioma ?? "pt_BR",
                    Telefones = contatos.Select(c => c.Telefone).ToList(),
                    ContatosIds = contatos.Select(c => c.Id.ToString()).ToList()
                };

                PreencherParametrosVariaveis(comandoLote, agendamento.Variaveis, contatos);

                var resultadoEnvio = await _mediator.Send(comandoLote);

                if (resultadoEnvio is not null && !resultadoEnvio.HasValidations)
                {
                    sucessos = resultadoEnvio.Value.TotalSucesso;
                    falhas = resultadoEnvio.Value.TotalFalha;

                    foreach (var erro in resultadoEnvio.Value.RelatorioErros)
                    {
                        _logger.LogError("Agendamento {AgendamentoId}: falha ao enviar para {Telefone} -- {Motivo}",
                            agendamento.Id, erro.Key, erro.Value);
                    }

                    _logger.LogInformation("Agendamento {AgendamentoId} concluido: {Sucessos} sucesso(s), {Falhas} falha(s).",
                        agendamento.Id, sucessos, falhas);
                }
                else
                {
                    falhas = contatos.Count;
                    var motivo = resultadoEnvio is null
                        ? "O disparo em lote não retornou resposta."
                        : string.Join("; ", resultadoEnvio.Erros);
                    _logger.LogError("Agendamento {AgendamentoId}: falha ao disparar em lote -- {Motivo}", agendamento.Id, motivo);
                }
            }

            resultado.TotalContatos += contatos.Count;
            resultado.Sucessos += sucessos;
            resultado.Falhas += falhas;

            await _unitOfWork.Agendamento.IncluirExecucao(new Entidades.AgendamentoExecucao
            {
                AgendamentoId = agendamento.Id,
                TotalContatos = contatos.Count,
                Sucessos = sucessos,
                Falhas = falhas,
                Status = falhas == 0 ? Entidades.AgendamentoExecucao.Concluida : Entidades.AgendamentoExecucao.Erro
            });

            var proximaExecucao = AgendamentoRecorrencia.CalcularProximaExecucao(
                agendamento.DataReferencia, agendamento.ProximaExecucao, agendamento.TipoRecorrencia);

            // Passou da data de termino: desativa em vez de continuar agendando alem do que
            // o cliente pediu.
            var aindaAtivo = !agendamento.DataFim.HasValue || proximaExecucao <= agendamento.DataFim.Value;

            await _unitOfWork.Agendamento.FinalizarExecucao(agendamento.Id, proximaExecucao, aindaAtivo);
        }

        // Mesma logica da tela de Disparo em lote (DisparadorComponent.valorDaVariavel):
        // "nome"/"telefone" sao resolvidos por contato a cada execucao recorrente, "fixo" e o
        // mesmo texto pra todo mundo. Sem isto, o disparo em lote ia sem nenhum parametro e a
        // Meta recusava qualquer template com variavel no corpo.
        private static void PreencherParametrosVariaveis(
            EnviarMensagemTemplateMetaLoteCommand comandoLote,
            List<Entidades.AgendamentoVariavelDto> variaveis,
            List<Entidades.Contato> contatos)
        {
            if (variaveis == null || variaveis.Count == 0) return;

            comandoLote.ParametrosBody = variaveis
                .Select(v => v.Origem == "fixo" ? (v.ValorFixo ?? string.Empty).Trim() : string.Empty)
                .ToList();

            var personalizado = variaveis.Any(v => v.Origem != "fixo");
            if (!personalizado) return;

            foreach (var contato in contatos)
            {
                comandoLote.ParametrosBodyPorTelefone[contato.Telefone] = variaveis
                    .Select(v => ResolverValorVariavel(v, contato))
                    .ToList();
            }
        }

        private static string ResolverValorVariavel(Entidades.AgendamentoVariavelDto variavel, Entidades.Contato contato)
        {
            return variavel.Origem switch
            {
                "nome" => contato.Nome ?? string.Empty,
                "telefone" => contato.Telefone ?? string.Empty,
                _ => (variavel.ValorFixo ?? string.Empty).Trim()
            };
        }
    }
}
