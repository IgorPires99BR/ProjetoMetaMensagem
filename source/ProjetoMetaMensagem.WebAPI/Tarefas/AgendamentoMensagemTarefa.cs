using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Tarefas;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento;

namespace ProjetoMetaMensagem.WebAPI.Tarefas
{
    public class AgendamentoMensagemTarefa : IAgendamentoMensagemTarefa
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AgendamentoMensagemTarefa> _logger;

        public AgendamentoMensagemTarefa(
            IMediator mediator,
            ILogger<AgendamentoMensagemTarefa> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Executar(Guid agendamentoId)
        {
            try
            {
                _logger.LogInformation("Iniciando a execução do agendamento {AgendamentoId}...", agendamentoId);

                var command = new ProcessaAgendamentoCommand { AgendamentoId = agendamentoId };
                await _mediator.Send(command);

                _logger.LogInformation("Execução do agendamento {AgendamentoId} finalizada.", agendamentoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao processar o agendamento {AgendamentoId} no Hangfire.", agendamentoId);
                throw; // Lança a exceção para que o Hangfire registre a falha e execute a política de retry.
            }
        }
    }
}
