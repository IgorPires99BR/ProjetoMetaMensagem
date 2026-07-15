using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook
{
    public class DeletaWebhookCommand : IRequest<Response<DeletaWebhookResult>>
    {
        public Guid Id { get; set; }
    }
}
