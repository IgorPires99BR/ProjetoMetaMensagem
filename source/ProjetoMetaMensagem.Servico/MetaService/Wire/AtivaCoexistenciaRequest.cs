using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class AtivaCoexistenciaRequest
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = "whatsapp";

        [JsonProperty("pin")]
        public string Pin { get; set; }
    }
}
