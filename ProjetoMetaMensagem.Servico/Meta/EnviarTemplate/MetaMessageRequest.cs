using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.EnviarTemplate
{
    public class MetaMessageRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct => "whatsapp";
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("type")]
        public string Type => "template";
        [JsonProperty("template")]
        public TemplateRequest Template { get; set; }
    }

    public class TemplateRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("language")]
        public LanguageRequest Language { get; set; }
    }

    public class LanguageRequest 
    { 
        [JsonProperty("code")] 
        public string Code { get; set; } = "pt_BR"; 
    }
}
