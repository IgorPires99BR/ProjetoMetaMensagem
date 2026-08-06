using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template
{
    public class CreateTemplateRequisicao
    {
        // Construtor mantido e ajustado com LINQ
        public CreateTemplateRequisicao(CriarTemplateMetaCommand command)
        {
            if (command == null) return;

            Nome = command.Nome;
            Idioma = command.Idioma ?? "pt_BR";
            Categoria = command.Categoria ?? "MARKETING";

            if (command.Componentes != null)
            {
                Componentes = command.Componentes.Select(c => new ComponenteTemplate
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

        // Exigido pela Meta quando o componente tem variável ({{n}} no BODY) ou é
        // mídia no HEADER — sem isso a Meta rejeita a criação do template.
        [JsonProperty("example", NullValueHandling = NullValueHandling.Ignore)]
        public TemplateExample Example { get; set; }
    }

    public class TemplateExample
    {
        // Handle retornado pela Resumable Upload API da Meta (não aceita URL direta)
        [JsonProperty("header_handle", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> HeaderHandle { get; set; }

        // Um conjunto de valores de exemplo, na ordem das variáveis {{1}}, {{2}}...
        [JsonProperty("body_text", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> BodyText { get; set; }
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
