using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.Meta.Numeros.AtivaCoexistencia
{
    public class AtivaCoexistenciaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
