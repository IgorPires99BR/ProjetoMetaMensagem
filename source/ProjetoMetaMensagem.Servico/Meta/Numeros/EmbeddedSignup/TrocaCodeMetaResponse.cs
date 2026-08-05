using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.Meta.Numeros.EmbeddedSignup
{
    public class TrocaCodeMetaResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
