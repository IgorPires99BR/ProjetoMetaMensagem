using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Templates.ObtemTemplateMeta
{
    public class ObtemTemplateMetaResponse
    {
        [JsonProperty("data")]
        public List<TemplateMetaResponseData> Data { get; set; } = new();
    }

    public class TemplateMetaResponseData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("components")]
        public List<TemplateComponentResponse> Components { get; set; } = new();
    }

    public class TemplateComponentResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; } // HEADER, BODY, FOOTER, BUTTONS

        [JsonProperty("text")]
        public string Text { get; set; } // O texto contendo as variáveis {{1}}, {{2}}
    }
}
