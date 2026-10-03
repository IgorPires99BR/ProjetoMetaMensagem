using Hangfire;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Tarefas;
using ProjetoMetaMensagem.Dominio.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Tarefas
{
    public class AgendamentoTarefaAgendador : IAgendamentoTarefaAgendador
    {
        private readonly IRecurringJobManager _recurringJobs;
        private readonly ILogger<AgendamentoTarefaAgendador> _logger;

        public AgendamentoTarefaAgendador(IRecurringJobManager recurringJobs, ILogger<AgendamentoTarefaAgendador> logger)
        {
            _recurringJobs = recurringJobs;
            _logger = logger;
        }

        public static string IdDoJob(Guid agendamentoId) => $"agendamento-{agendamentoId}";

        public void Registrar(Agendamento agendamento)
        {
            if (!agendamento.Ativo)
            {
                Remover(agendamento.Id);
                return;
            }

            var agendamentoId = agendamento.Id;

            // Fuso local: o HangFire avalia cron em UTC por padrao, mas DataReferencia e
            // ProximaExecucao sao gravadas/comparadas com DateTime.Now -- sem isto o job acordaria
            // horas antes/depois da ProximaExecucao e nunca a encontraria devida.
            _recurringJobs.AddOrUpdate<AgendamentoMensagemTarefa>(
                IdDoJob(agendamentoId),
                tarefa => tarefa.Executar(agendamentoId),
                AgendamentoRecorrencia.ExpressaoCron(agendamento),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        }

        public void Remover(Guid agendamentoId) => _recurringJobs.RemoveIfExists(IdDoJob(agendamentoId));

        public void ExecutarAgora(Guid agendamentoId)
        {
            try
            {
                _recurringJobs.Trigger(IdDoJob(agendamentoId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nao foi possivel antecipar a execucao do agendamento {AgendamentoId}; sai no proximo horario do cron.",
                    agendamentoId);
            }
        }
    }
}
