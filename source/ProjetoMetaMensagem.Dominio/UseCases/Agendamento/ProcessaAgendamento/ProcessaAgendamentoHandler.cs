using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Tarefas;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Chamado pela AgendamentoMensagemTarefa (job recorrente do HangFire, um por agendamento) via
    // mediator -- toda a logica de negocio do processamento mora aqui, seguindo o mesmo desenho
    // Command/Handler dos demais casos de uso; a tarefa em si so dispara o comando.
    public class ProcessaAgendamentoHandler : IRequestHandler<ProcessaAgendamentoCommand, Response<ProcessaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IAgendamentoTarefaAgendador _agendador;
        private readonly ILogger<ProcessaAgendamentoHandler> _logger;

        // O cron acorda no minuto cheio, mas a DataReferencia pode ter segundos -- sem a folga, o
        // job das 08:00:00 acharia a ProximaExecucao 08:00:30 "no futuro" e pularia o disparo.
        private static readonly TimeSpan FolgaDoCron = TimeSpan.FromMinutes(1);

        public ProcessaAgendamentoHandler(
            IUnitOfWork unitOfWork, IMediator mediator, IAgendamentoTarefaAgendador agendador,
            ILogger<ProcessaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _agendador = agendador;
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
                var agendamento = await _unitOfWork.Agendamento.ObterPorId(command.AgendamentoId, null);
                if (agendamento == null || !agendamento.Ativo)
                {
                    // Excluido/pausado sem o job ter saido do HangFire (ex: falha entre o Commit e
                    // o HangFire): o proprio job se desliga em vez de seguir acordando a toa.
                    _logger.LogInformation("Agendamento {AgendamentoId} inexistente ou inativo; removendo o job do HangFire.",
                        command.AgendamentoId);
                    _agendador.Remover(command.AgendamentoId);
                    response.AddValue(resultado);
                    return response;
                }

                // Job acordou fora da vez (ex: mensal dia 31 acorda de 28 a 31, ver
                // AgendamentoRecorrencia.ExpressaoCron): quem manda e a ProximaExecucao.
                if (agendamento.ProximaExecucao > DateTime.Now.Add(FolgaDoCron))
                {
                    _logger.LogInformation("Agendamento {AgendamentoId} ainda nao esta devido (proxima execucao {ProximaExecucao}).",
                        agendamento.Id, agendamento.ProximaExecucao);
                    response.AddValue(resultado);
                    return response;
                }

                // Reserva por prazo: se outra execucao ja pegou este agendamento (disparo manual
                // pelo dashboard junto com o cron, retry do HangFire), pula sem reenviar.
                var prazoProcessamento = DateTime.Now.AddMinutes(10);
                if (!await _unitOfWork.Agendamento.ReivindicarAgendamento(agendamento.Id, prazoProcessamento))
                {
                    _logger.LogInformation("Agendamento {AgendamentoId} ja estava sendo processado por outra execucao; pulando.",
                        agendamento.Id);
                    response.AddValue(resultado);
                    return response;
                }

                resultado.TotalAgendamentos = 1;
                await ProcessarUmAgendamento(agendamento, resultado);

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

            // VencimentoContato: a lista do agendamento pode ter contato de qualquer dia de
            // vencimento -- so quem "vence" hoje (com o mesmo clamp de fim de mes que a
            // recorrencia Mensal ja usa pro DiaDoMes) recebe disparo nesta passada. Os demais
            // ficam pra quando o dia deles chegar, sem precisar de agendamentos separados.
            var totalDaLista = contatos.Count;
            if (agendamento.TipoRecorrencia == Entidades.Agendamento.VencimentoContato)
            {
                contatos = contatos.Where(c => VenceHoje(c.DiaVencimento, DateTime.Now)).ToList();
            }

            _logger.LogInformation("Agendamento {AgendamentoId} ({Nome}): {Total} contato(s) para disparo{DoTotal}.",
                agendamento.Id, agendamento.Nome, contatos.Count,
                agendamento.TipoRecorrencia == Entidades.Agendamento.VencimentoContato ? $" de {totalDaLista} na lista" : "");

            // Ninguem vence hoje: nao ha o que disparar, e registrar uma AgendamentoExecucao
            // 0/0/0 toda santa noite so faria ruido no historico sem informar nada de util.
            if (agendamento.TipoRecorrencia == Entidades.Agendamento.VencimentoContato && contatos.Count == 0)
            {
                await AgendarProxima(agendamento);
                return;
            }

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
                    ContatosIds = contatos.Select(c => c.Id.ToString()).ToList(),
                    Origem = ProjetoMetaMensagem.Dominio.Common.OrigemDisparo.AgendadorAutomatico
                };

                // Sem os parametros o disparo em lote ia sem nenhum valor de variavel e a Meta
                // recusava qualquer template com variavel no corpo. "hoje" fixa o mes usado
                // por dataVencimento: o da execucao, nao o da criacao do agendamento.
                var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(agendamento.EmpresaId))
                    .ToDictionary(p => p.Id);

                ResolvedorDeVariaveis.Preencher(
                    comandoLote, agendamento.Variaveis,
                    contatos.Select(c => (c.Telefone, c)), parametros, DateTime.Now);

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

            resultado.TotalContatos = contatos.Count;
            resultado.Sucessos = sucessos;
            resultado.Falhas = falhas;

            await _unitOfWork.Agendamento.IncluirExecucao(new Entidades.AgendamentoExecucao
            {
                AgendamentoId = agendamento.Id,
                TotalContatos = contatos.Count,
                Sucessos = sucessos,
                Falhas = falhas,
                Status = falhas == 0 ? Entidades.AgendamentoExecucao.Concluida : Entidades.AgendamentoExecucao.Erro
            });

            await AgendarProxima(agendamento);
        }

        private async Task AgendarProxima(Entidades.Agendamento agendamento)
        {
            var proximaExecucao = AgendamentoRecorrencia.CalcularProximaExecucaoFutura(agendamento, DateTime.Now);

            // Passou da data de termino: desativa em vez de continuar agendando alem do que
            // o cliente pediu -- e tira o job do HangFire, que nao tem mais o que disparar.
            var aindaAtivo = !agendamento.DataFim.HasValue || proximaExecucao <= agendamento.DataFim.Value;

            await _unitOfWork.Agendamento.FinalizarExecucao(agendamento.Id, proximaExecucao, aindaAtivo);

            if (!aindaAtivo)
                _agendador.Remover(agendamento.Id);
        }

        // Contato "vence" hoje quando o dia dele bate com o dia do mes atual -- clampado pro
        // ultimo dia do mes quando o vencimento (ex: 31) nao existir no mes corrente (ex:
        // fevereiro), mesmo criterio que a recorrencia Mensal ja usa pro DiaDoMes fixo.
        private static bool VenceHoje(int? diaVencimento, DateTime hoje)
        {
            if (!diaVencimento.HasValue) return false;

            var ultimoDiaDoMes = DateTime.DaysInMonth(hoje.Year, hoje.Month);
            var diaEfetivo = Math.Min(diaVencimento.Value, ultimoDiaDoMes);
            return diaEfetivo == hoje.Day;
        }
    }
}
