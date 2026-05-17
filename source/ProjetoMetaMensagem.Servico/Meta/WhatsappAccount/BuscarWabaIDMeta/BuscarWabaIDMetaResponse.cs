using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.WhatsappAccount.BuscarWabaIDMeta
{
    public class BuscarWabaIDMetaResponse
    {
        public List<WabaDataInternalDto> Data { get; set; } = new();
    }

    public class WabaDataInternalDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
