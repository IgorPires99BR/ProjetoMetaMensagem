using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.WebAPI.Hubs
{
    public class ChatHub : Hub
    {
        // Nome do metodo precisa bater exatamente com o invoke() feito no Angular (chats.component.ts)
        public async Task EntrarNoGrupo(string empresaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, empresaId);
        }
    }
}
