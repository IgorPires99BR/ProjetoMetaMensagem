using Microsoft.AspNetCore.SignalR;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Hubs
{
    public class NotificadorChat : INotificadorChat
    {
        private readonly IHubContext<ChatHub> _hub;
        private readonly ILogger<NotificadorChat> _logger;

        public NotificadorChat(IHubContext<ChatHub> hub, ILogger<NotificadorChat> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task NotificarMensagemEnviadaAsync(Guid empresaId, Guid contatoId, string conteudo, string? wamid)
        {
            try
            {
                // Mesmo grupo por empresa que o webhook usa, e um evento próprio: assim o painel
                // sabe que a bolha é da plataforma (lado direito) e não do cliente.
                await _hub.Clients.Group(empresaId.ToString())
                    .SendAsync("ReceberMensagemEnviada", new
                    {
                        contatoId,
                        conteudo,
                        wamid,
                        dataEnvio = DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                // Avisar a tela é acessório: a mensagem já foi enviada ao cliente.
                _logger.LogError(ex, "Falha ao notificar o painel sobre a mensagem enviada ao contato {ContatoId}", contatoId);
            }
        }
    }
}
