using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class CriaNumeroMetaResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } // O Phone Number ID gerado pela Meta
    }
}
