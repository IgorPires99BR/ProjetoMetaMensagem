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

        public async Task Executar()
        {
            try
            {
                _logger.LogInformation("Iniciando a execução da tarefa de agendamento de mensagens...");

                var command = new ProcessaAgendamentoCommand();
                await _mediator.Send(command);

                _logger.LogInformation("Tarefa de agendamento de mensagens executada com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao processar o agendamento de mensagens no Hangfire.");
                throw; // Lança a exceção para que o Hangfire registre a falha e execute a política de retry.
            }
        }
    }
}
