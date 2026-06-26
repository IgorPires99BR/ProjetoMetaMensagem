using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook
{
    public class ListaWebhookCommand : IRequest<Response<List<ListaWebhookResult>>>
    {
        public Guid EmpresaId { get; set; }
    }
}
