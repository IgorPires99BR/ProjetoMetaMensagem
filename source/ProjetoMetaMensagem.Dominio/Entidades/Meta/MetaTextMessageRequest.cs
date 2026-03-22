using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Meta
{
    public class MetaTextMessageRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("recipient_type")]
        public string RecipientType { get; set; } = "individual";

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public TextContent Text { get; set; }
    }

    public class TextContent
    {
        [JsonProperty("preview_url")]
        public bool PreviewUrl { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
