using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    public class CreateTemplateRequest
    {
        public CreateTemplateRequest(string nome, string idioma, string categoria, List<ComponenteTemplateEnvio> componentes)
        {
            Name = nome;
            Language = idioma;
            Category = categoria;

            if (componentes != null)
            {
                Components = componentes.Select(c => new TemplateComponent
                {
                    Type = c.Tipo,
                    Format = c.Formato,
                    Text = c.Texto,
                    Buttons = c.Botoes?.Select(b => new TemplateButtonDTO
                    {
                        Type = b.Tipo,
                        Text = b.Texto,
                        Url = b.Url,
                        PhoneNumber = b.NumeroTelefone
                    }).ToList(),
                    Example = (c.HeaderHandle != null || c.BodyTextExemplos != null)
                        ? new TemplateExampleRequest { HeaderHandle = c.HeaderHandle, BodyText = c.BodyTextExemplos }
                        : null
                }).ToList();
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; } = "pt_BR";

        [JsonProperty("category")]
        public string Category { get; set; } = "MARKETING"; // Opções: MARKETING, UTILITY, AUTHENTICATION

        [JsonProperty("components")]
        public List<TemplateComponent> Components { get; set; }
    }

    public class TemplateComponent
    {
        [JsonProperty("type")]
        public string Type { get; set; } // HEADER, BODY, FOOTER, BUTTONS

        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; } // Apenas para HEADER (TEXT, IMAGE, etc)

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public List<TemplateButtonDTO> Buttons { get; set; }

        // Exigido pela Meta quando o componente tem variável ({{n}} no BODY) ou é
        // mídia no HEADER — sem isso a Meta rejeita a criação do template.
        [JsonProperty("example", NullValueHandling = NullValueHandling.Ignore)]
        public TemplateExampleRequest Example { get; set; }
    }

    public class TemplateExampleRequest
    {
        // Handle retornado pela Resumable Upload API da Meta (não aceita URL direta)
        [JsonProperty("header_handle", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HeaderHandle { get; set; }

        // Um conjunto de valores de exemplo, na ordem das variáveis {{1}}, {{2}}...
        [JsonProperty("body_text", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> BodyText { get; set; }
    }

    public class TemplateButtonDTO
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("phone_number", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber { get; set; }
    }
}
