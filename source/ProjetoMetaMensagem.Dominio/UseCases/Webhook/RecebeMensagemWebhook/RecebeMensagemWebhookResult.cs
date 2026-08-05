using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook
{
    public class RecebeMensagemWebhookResult
    {
        public bool Sucesso{ get; set; }
        public string Mensagem{ get; set; }

        // Mensagens efetivamente salvas nesta chamada do webhook, usadas pelo
        // controller pra fazer o broadcast em tempo real via SignalR, uma por empresa/contato.
        public List<MensagemRecebidaBroadcastDto> MensagensSalvas { get; set; } = new();
    }

    public class MensagemRecebidaBroadcastDto
    {
        public Guid EmpresaId { get; set; }
        public Guid? ContatoId { get; set; }
        public string TelefoneRemetente { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataRecebimento { get; set; }
    }
}
