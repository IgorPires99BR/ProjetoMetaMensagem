using Microsoft.AspNetCore.SignalR;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Hubs
{
    public class NotificadorCobranca : INotificadorCobranca
    {
        private readonly IHubContext<ChatHub> _hub;
        private readonly ILogger<NotificadorCobranca> _logger;

        public NotificadorCobranca(IHubContext<ChatHub> hub, ILogger<NotificadorCobranca> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task NotificarCobrancaAtualizadaAsync(Guid empresaId, Guid cobrancaId, string status)
        {
            try
            {
                // So o id e o status: a tela busca o resto pela API, que ja aplica o recorte por
                // empresa e por perfil (o hub so garante a empresa).
                await _hub.Clients.Group(empresaId.ToString())
                    .SendAsync("CobrancaAtualizada", new { cobrancaId, status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao avisar o painel sobre a cobranca {CobrancaId}", cobrancaId);
            }
        }
    }
}
