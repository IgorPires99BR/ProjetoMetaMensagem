using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class TrocaTokenLongLivedResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        // Segundos até expirar (~5184000 = 60 dias pro token long-lived). Ausente = a Meta
        // não informou validade (tratado como token sem expiração conhecida).
        [JsonProperty("expires_in")]
        public long? ExpiresIn { get; set; }
    }
}
