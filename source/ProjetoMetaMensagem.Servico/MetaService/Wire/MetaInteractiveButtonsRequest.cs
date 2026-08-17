using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    // Mensagem interativa de sessao (nao e template, nao precisa de aprovacao da Meta) --
    // so pode ser enviada dentro da janela de 24h de uma conversa ja iniciada pelo cliente.
    public class MetaInteractiveButtonsRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("recipient_type")]
        public string RecipientType { get; set; } = "individual";

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "interactive";

        [JsonProperty("interactive")]
        public InteractiveButtonsContent Interactive { get; set; }
    }

    public class InteractiveButtonsContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "button";

        [JsonProperty("body")]
        public InteractiveBody Body { get; set; }

        [JsonProperty("action")]
        public InteractiveButtonsAction Action { get; set; }
    }

    public class InteractiveBody
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class InteractiveButtonsAction
    {
        [JsonProperty("buttons")]
        public List<InteractiveButtonWrapper> Buttons { get; set; } = new List<InteractiveButtonWrapper>();
    }

    public class InteractiveButtonWrapper
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "reply";

        [JsonProperty("reply")]
        public InteractiveButtonReply Reply { get; set; }
    }

    public class InteractiveButtonReply
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
