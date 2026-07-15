using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Servico.Meta.Templates.ObtemTemplateMeta
{
    public class ObtemTemplateMetaResponse
    {
        [JsonProperty("data")]
        public List<TemplateMetaResponseData> Data { get; set; } = new();

        [JsonProperty("paging")]
        public PagingResponse Paging { get; set; }
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

        [JsonProperty("sub_category", NullValueHandling = NullValueHandling.Ignore)]
        public string SubCategory { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("parameter_format")]
        public string ParameterFormat { get; set; } // POSITIONAL ou NAMED

        [JsonProperty("components")]
        public List<TemplateComponentResponse> Components { get; set; } = new();
    }

    public class TemplateComponentResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; } // HEADER, BODY, FOOTER, BUTTONS

        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; } // CRÍTICO: IMAGE, VIDEO, DOCUMENT, TEXT (Vem quando Type for HEADER)

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; } // O texto bruto ou contendo variáveis {{1}}, {{2}}

        [JsonProperty("example", NullValueHandling = NullValueHandling.Ignore)]
        public ComponentExampleResponse Example { get; set; }

        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public List<ButtonResponse> Buttons { get; set; } // Lista de botões reais quando Type for BUTTONS
    }

    public class ComponentExampleResponse
    {
        [JsonProperty("header_handle", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HeaderHandle { get; set; } // URLs temporárias das mídias padrão da Meta

        [JsonProperty("body_text", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> BodyText { get; set; } // Textos de exemplo para as variáveis do corpo
    }

    public class ButtonResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; } // QUICK_REPLY, URL, PHONE_NUMBER

        [JsonProperty("text")]
        public string Text { get; set; } // Texto escrito dentro do botão

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; } // Link base do botão se o tipo for URL
    }

    public class PagingResponse
    {
        [JsonProperty("cursors")]
        public CursorsResponse Cursors { get; set; }
    }

    public class CursorsResponse
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}