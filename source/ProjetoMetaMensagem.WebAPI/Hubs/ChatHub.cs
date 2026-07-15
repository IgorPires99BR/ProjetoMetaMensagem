using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.WebAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinCompanyGroup(string empresaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, empresaId);
        }
    }
}
