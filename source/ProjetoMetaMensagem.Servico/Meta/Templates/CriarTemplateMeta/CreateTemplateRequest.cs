using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Templates.CriarTemplate
{
    public class CreateTemplateRequest
    {
        public CreateTemplateRequest(CreateTemplateRequisicao entidade)
        {
            if (entidade == null) return;

            Name = entidade.Nome;
            Language = entidade.Idioma;
            Category = entidade.Categoria;

            if (entidade.Componentes != null)
            {
                Components = entidade.Componentes.Select(c => new TemplateComponent
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
                    }).ToList()
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

        // Adicionado para suportar o mapeamento completo da entidade

    }

    public class TemplateComponent
    {
        [JsonProperty("type")]
        public string Type { get; set; } // HEADER, BODY, FOOTER, BUTTONS

        [JsonProperty("format")]
        public string Format { get; set; } // Apenas para HEADER (TEXT, IMAGE, etc)

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("buttons")]
        public List<TemplateButtonDTO> Buttons { get; set; }
    }

    public class TemplateButtonDTO
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
    }
}
