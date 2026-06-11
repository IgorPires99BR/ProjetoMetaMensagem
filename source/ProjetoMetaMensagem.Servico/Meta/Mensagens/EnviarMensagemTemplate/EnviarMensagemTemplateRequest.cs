using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Mensagens.EnviarMensagemTemplate
{
    public class EnviarMensagemTemplateRequest
    {
        public string MessagingProduct { get; set; } = "whatsapp";
        public string To { get; set; }
        public string Type { get; set; } = "template";
        public TemplateDataRequest Template { get; set; }
    }

    public class TemplateDataRequest
    {
        public string Name { get; set; }
        public LanguageDataRequest Language { get; set; }
        public List<object> Components { get; set; }
    }

    public class LanguageDataRequest
    {
        public string Code { get; set; }
    }
}
