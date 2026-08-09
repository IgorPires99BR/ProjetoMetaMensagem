using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class TrocaCodeMetaResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
