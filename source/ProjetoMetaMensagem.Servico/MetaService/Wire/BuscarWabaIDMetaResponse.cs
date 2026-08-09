using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
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
