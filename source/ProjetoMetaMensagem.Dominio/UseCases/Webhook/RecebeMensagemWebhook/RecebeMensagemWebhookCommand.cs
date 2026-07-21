using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook
{
    public class RecebeMensagemWebhookCommand : IRequest<Response<RecebeMensagemWebhookResult>>
    {
        [JsonPropertyName("object")]
        [JsonProperty("object")]
        public string? Objeto { get; set; }

        [JsonPropertyName("entry")]
        [JsonProperty("entry")]
        public List<WebhookEntryCommand> Entry { get; set; } = new List<WebhookEntryCommand>();
    }

    public class WebhookEntryCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("changes")]
        [JsonProperty("changes")]
        public List<WebhookChangeCommand> Changes { get; set; } = new List<WebhookChangeCommand>();
    }

    public class WebhookChangeCommand
    {
        [JsonPropertyName("field")]
        [JsonProperty("field")]
        public string? Field { get; set; }

        [JsonPropertyName("value")]
        [JsonProperty("value")]
        public WebhookValueCommand? Value { get; set; }
    }

    public class WebhookValueCommand
    {
        [JsonPropertyName("messaging_product")]
        [JsonProperty("messaging_product")]
        public string? MessagingProduct { get; set; }

        [JsonPropertyName("metadata")]
        [JsonProperty("metadata")]
        public WebhookMetadataCommand? Metadata { get; set; }

        [JsonPropertyName("contacts")]
        [JsonProperty("contacts")]
        public List<WebhookContactCommand> Contacts { get; set; } = new List<WebhookContactCommand>();

        [JsonPropertyName("messages")]
        [JsonProperty("messages")]
        public List<WebhookMessageCommand> Messages { get; set; } = new List<WebhookMessageCommand>();
    }

    public class WebhookMetadataCommand
    {
        [JsonPropertyName("display_phone_number")]
        [JsonProperty("display_phone_number")]
        public string? DisplayPhoneNumber { get; set; }

        [JsonPropertyName("phone_number_id")]
        [JsonProperty("phone_number_id")]
        public string? PhoneNumberId { get; set; }
    }

    public class WebhookContactCommand
    {
        [JsonPropertyName("wa_id")]
        [JsonProperty("wa_id")]
        public string? WaId { get; set; }

        [JsonPropertyName("user_id")]
        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("profile")]
        [JsonProperty("profile")]
        public WebhookProfileCommand? Profile { get; set; }
    }

    public class WebhookProfileCommand
    {
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class WebhookMessageCommand
    {
        [JsonPropertyName("from")]
        [JsonProperty("from")]
        public string? From { get; set; }

        [JsonPropertyName("from_user_id")]
        [JsonProperty("from_user_id")]
        public string? FromUserId { get; set; }

        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("timestamp")]
        [JsonProperty("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("type")]
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        [JsonProperty("text")]
        public WebhookTextCommand? Text { get; set; }

        [JsonPropertyName("image")]
        [JsonProperty("image")]
        public WebhookImageCommand? Image { get; set; }
    }

    public class WebhookTextCommand
    {
        [JsonPropertyName("body")]
        [JsonProperty("body")]
        public string? Body { get; set; }
    }

    public class WebhookImageCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }
    }
}