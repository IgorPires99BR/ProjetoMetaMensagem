using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Numeros.CriaNumeroMeta
{
    public class CriaNumeroMetaResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } // O Phone Number ID gerado pela Meta
    }
}
