using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook
{
    public class CriaWebhookCommand : IRequest<Response<CriaWebhookResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string Url { get; set; }
        public string Evento { get; set; }
    }
}
