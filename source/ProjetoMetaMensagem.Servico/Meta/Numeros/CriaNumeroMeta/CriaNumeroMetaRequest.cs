using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Numeros.CriaNumeroMeta
{
    public class CriaNumeroMetaRequest
    {

        public CriaNumeroMetaRequest(CriaNumeroMetaRequisicao requisicao)
        {
            PhoneNumber = requisicao.Telefone;
            VerifiedName = requisicao.NomeVerificado.ToString();
            Cc = requisicao.CodigoPais;
        }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("verified_name")]
        public string VerifiedName { get; set; }

        [JsonProperty("cc")]
        public string Cc { get; set; } // Código do país (Ex: "55" para Brasil)
    }
}
