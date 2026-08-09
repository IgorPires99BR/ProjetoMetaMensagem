using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class AtivaCoexistenciaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
