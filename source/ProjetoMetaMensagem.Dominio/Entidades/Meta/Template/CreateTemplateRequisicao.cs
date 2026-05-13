using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Entidades.Meta.Template
{
    public class CreateTemplateRequisicao
    {
            // Construtor mantido e ajustado com LINQ
            public CreateTemplateRequisicao(CriarTemplateMetaCommand command)
            {
                if (command == null) return;

                this.Nome = command.Nome;
                this.Idioma = command.Idioma ?? "pt_BR";
                this.Categoria = command.Categoria ?? "MARKETING";

                if (command.Componentes != null)
                {
                    this.Componentes = command.Componentes.Select(c => new ComponenteTemplate
                    {
                        Tipo = c.Tipo,
                        Formato = c.Formato,
                        Texto = c.Texto,
                        Botoes = c.Botoes?.Select(b => new BotaoTemplate
                        {
                            Tipo = b.Tipo,
                            Texto = b.Texto,
                            Url = b.Url,
                            NumeroTelefone = b.NumeroTelefone
                        }).ToList()
                    }).ToList();
                }
            }

            // Construtor vazio necessário para desserialização se houver
            public CreateTemplateRequisicao() { }

            [JsonProperty("name")]
            public string Nome { get; set; }

            [JsonProperty("language")]
            public string Idioma { get; set; }

            [JsonProperty("category")]
            public string Categoria { get; set; }

            [JsonProperty("components")]
            public List<ComponenteTemplate> Componentes { get; set; }
        }

        public class ComponenteTemplate
        {
            [JsonProperty("type")]
            public string Tipo { get; set; } // HEADER, BODY, FOOTER, BUTTONS

            [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
            public string Formato { get; set; } // TEXT, IMAGE, DOCUMENT, VIDEO

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Texto { get; set; }

            [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
            public List<BotaoTemplate> Botoes { get; set; }
        }

        public class BotaoTemplate
        {
            [JsonProperty("type")]
            public string Tipo { get; set; } // QUICK_REPLY, PHONE_NUMBER, URL

            [JsonProperty("text")]
            public string Texto { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            [JsonProperty("phone_number", NullValueHandling = NullValueHandling.Ignore)]
            public string NumeroTelefone { get; set; }
        }
    }
