using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Mensagens.EnviarMensagemTemplate
{
    public class EnviarMensagemTemplateResponse
    {
        public string MessagingProduct { get; set; }
        public List<ContactResponseData> Contacts { get; set; }
        public List<MessageResponseData> Messages { get; set; }
    }

    public class ContactResponseData
    {
        public string Input { get; set; }
        public string WaId { get; set; }
    }

    public class MessageResponseData
    {
        public string Id { get; set; } // Este é o wamid recebido da Meta
    }
}
