using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProjetoMetaMensagem.WebAPI.Hubs
{
    // Canal em tempo real por empresa: mensagens recebidas/enviadas do Chats, status de entrega e
    // avisos de cobranca paga. O grupo de cada conexao vem SEMPRE do JWT: antes o hub era aberto
    // e EntrarNoGrupo aceitava qualquer empresaId, entao quem soubesse o id de uma empresa ouvia
    // as conversas dela sem login.
    //
    // O token chega pela query access_token (WebSocket nao manda cabecalho), lido no
    // OnMessageReceived do JWT em Program.cs.
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var empresaId = EmpresaDoToken();
            if (empresaId == null)
            {
                // Token valido mas sem empresa (nao deveria existir): nao entra em grupo nenhum.
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, empresaId);
            await base.OnConnectedAsync();
        }

        // Mantido porque o front ja chama (chats e cobrancas-clientes, nome tem que bater com o
        // invoke). A empresa do token ja entrou no OnConnectedAsync; outra empresa so para o
        // admin da plataforma, que enxerga todas.
        public async Task EntrarNoGrupo(string empresaId)
        {
            if (!Guid.TryParse(empresaId, out var id))
                throw new HubException("Empresa inválida.");

            var daPropriaEmpresa = string.Equals(id.ToString(), EmpresaDoToken(), StringComparison.OrdinalIgnoreCase);
            if (!daPropriaEmpresa && !EhAdminDaPlataforma())
                throw new HubException("Sem acesso a esta empresa.");

            // Mesmo formato usado pelos envios (Guid.ToString()), senao o grupo nao casa.
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
        }

        private string? EmpresaDoToken() =>
            Guid.TryParse(Context.User?.FindFirst("empresaId")?.Value, out var id) ? id.ToString() : null;

        private bool EhAdminDaPlataforma() =>
            string.Equals(Context.User?.FindFirst("isAdminPlataforma")?.Value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
