using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    // Corpo do POST /{phone-number-id}/whatsapp_business_profile. Serializado com
    // NullValueHandling.Ignore: campo nulo nao vai, e a Meta mantem o valor atual dele.
    public class AtualizaPerfilNumeroRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("about")]
        public string? About { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("websites")]
        public List<string>? Websites { get; set; }

        [JsonProperty("vertical")]
        public string? Vertical { get; set; }

        [JsonProperty("profile_picture_handle")]
        public string? ProfilePictureHandle { get; set; }
    }

    // GET /{phone-number-id}/whatsapp_business_profile devolve o perfil dentro de data[0].
    public class ObtemPerfilNumeroResponse
    {
        [JsonProperty("data")]
        public List<PerfilNumeroMetaWire>? Data { get; set; }
    }

    public class PerfilNumeroMetaWire
    {
        [JsonProperty("about")]
        public string? About { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("websites")]
        public List<string>? Websites { get; set; }

        [JsonProperty("vertical")]
        public string? Vertical { get; set; }

        [JsonProperty("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }

    public class NomeNumeroMetaResponse
    {
        [JsonProperty("verified_name")]
        public string? VerifiedName { get; set; }

        [JsonProperty("new_display_name")]
        public string? NewDisplayName { get; set; }

        [JsonProperty("new_name_status")]
        public string? NewNameStatus { get; set; }
    }
}
