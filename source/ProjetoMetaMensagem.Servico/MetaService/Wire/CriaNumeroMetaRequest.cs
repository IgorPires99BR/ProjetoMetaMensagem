using Newtonsoft.Json;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class CriaNumeroMetaRequest
    {
        public CriaNumeroMetaRequest(string telefone, string nomeVerificado, string codigoPais)
        {
            PhoneNumber = telefone;
            VerifiedName = nomeVerificado;
            Cc = codigoPais;
        }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("verified_name")]
        public string VerifiedName { get; set; }

        [JsonProperty("cc")]
        public string Cc { get; set; } // Código do país (Ex: "55" para Brasil)
    }
}
