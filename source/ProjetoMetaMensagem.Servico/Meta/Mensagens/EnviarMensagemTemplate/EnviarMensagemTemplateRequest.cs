using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.Meta.Mensagens.EnviarMensagemTemplate
{
    public class EnviarMensagemTemplateRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "template";

        [JsonProperty("template")]
        public TemplateDataRequest Template { get; set; }
    }

    public class TemplateDataRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("language")]
        public LanguageDataRequest Language { get; set; }

        [JsonProperty("components")]
        public List<object> Components { get; set; }
    }

    public class LanguageDataRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}