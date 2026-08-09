using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
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
        public List<ComponentDataRequest> Components { get; set; }
    }

    public class ComponentDataRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sub_type", NullValueHandling = NullValueHandling.Ignore)]
        public string SubType { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public string Index { get; set; }

        [JsonProperty("parameters")]
        public List<ParameterDataRequest> Parameters { get; set; }
    }

    public class ParameterDataRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public MediaLinkDataRequest Image { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public MediaLinkDataRequest Video { get; set; }

        [JsonProperty("document", NullValueHandling = NullValueHandling.Ignore)]
        public MediaLinkDataRequest Document { get; set; }
    }

    public class LanguageDataRequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class MediaLinkDataRequest
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
