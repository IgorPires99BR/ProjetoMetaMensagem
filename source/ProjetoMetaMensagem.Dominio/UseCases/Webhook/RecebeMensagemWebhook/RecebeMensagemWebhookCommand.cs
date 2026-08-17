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

        [JsonPropertyName("statuses")]
        [JsonProperty("statuses")]
        public List<WebhookStatusCommand> Statuses { get; set; } = new List<WebhookStatusCommand>();
    }

    public class WebhookStatusCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonPropertyName("timestamp")]
        [JsonProperty("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("recipient_id")]
        [JsonProperty("recipient_id")]
        public string? RecipientId { get; set; }

        // A Meta manda o motivo real da falha aqui quando status = "failed"
        // (ex: mídia inválida, número não whitelistado, janela de 24h expirada).
        [JsonPropertyName("errors")]
        [JsonProperty("errors")]
        public List<WebhookStatusErrorCommand> Errors { get; set; } = new();
    }

    public class WebhookStatusErrorCommand
    {
        [JsonPropertyName("code")]
        [JsonProperty("code")]
        public int? Code { get; set; }

        [JsonPropertyName("title")]
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonPropertyName("message")]
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error_data")]
        [JsonProperty("error_data")]
        public WebhookStatusErrorDataCommand? ErrorData { get; set; }
    }

    public class WebhookStatusErrorDataCommand
    {
        [JsonPropertyName("details")]
        [JsonProperty("details")]
        public string? Details { get; set; }
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

        [JsonPropertyName("audio")]
        [JsonProperty("audio")]
        public WebhookAudioCommand? Audio { get; set; }

        [JsonPropertyName("document")]
        [JsonProperty("document")]
        public WebhookDocumentCommand? Document { get; set; }

        // Resposta a um botao de template (type = "button"): a Meta manda o texto do botao
        // que o cliente tocou. Sem mapear isto, a resposta virava "[Midia do tipo button]" --
        // o texto se perdia e nenhum flow conseguia casar a palavra-chave dele.
        [JsonPropertyName("button")]
        [JsonProperty("button")]
        public WebhookButtonCommand? Button { get; set; }

        // Resposta a botao ou lista de mensagem interativa (type = "interactive").
        [JsonPropertyName("interactive")]
        [JsonProperty("interactive")]
        public WebhookInteractiveCommand? Interactive { get; set; }
    }

    public class WebhookButtonCommand
    {
        [JsonPropertyName("text")]
        [JsonProperty("text")]
        public string? Text { get; set; }

        // Valor configurado no template; usado quando o botao nao traz texto visivel.
        [JsonPropertyName("payload")]
        [JsonProperty("payload")]
        public string? Payload { get; set; }
    }

    public class WebhookInteractiveCommand
    {
        [JsonPropertyName("button_reply")]
        [JsonProperty("button_reply")]
        public WebhookReplyCommand? ButtonReply { get; set; }

        [JsonPropertyName("list_reply")]
        [JsonProperty("list_reply")]
        public WebhookReplyCommand? ListReply { get; set; }
    }

    public class WebhookReplyCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        [JsonProperty("title")]
        public string? Title { get; set; }
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

    public class WebhookAudioCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }
    }

    public class WebhookDocumentCommand
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }
    }
}