using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook
{
    public class ListaWebhookResult
    {
        public ListaWebhookResult(Entidades.WebhookConfig webhook)
        {
            Id = webhook.Id;
            EmpresaId = webhook.EmpresaId;
            Nome = webhook.Nome;
            Url = webhook.Url;
            Evento = webhook.Evento;
            Ativo = webhook.Ativo;
            DataCriacao = webhook.DataCriacao;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string Url { get; set; }
        public string Evento { get; set; }
        public bool Ativo { get; set; }
        public DateTimeOffset DataCriacao { get; set; }
    }
}
