using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate
{
    public class EnviarMensagemTemplateRequisicao
    {
        public EnviarMensagemTemplateRequisicao()
        {

        }
        public EnviarMensagemTemplateRequisicao(EnviarMensagemTemplateMetaCommand command)
        {
            if (command == null) return;

            Para = command.Telefone;
            Template = new TemplateData
            {
                Nome = command.NomeTemplate,
                Idioma = new LanguageData { Codigo = command.Idioma },
                Componentes = new List<ComponenteEnvio>()
            };

            // Mapeia parâmetros do BODY se existirem
            if (command.ParametrosBody != null && command.ParametrosBody.Any())
            {
                Template.Componentes.Add(new ComponenteEnvio
                {
                    Tipo = "body",
                    Parametros = command.ParametrosBody.Select(p => new ParametroEnvio { Texto = p }).ToList()
                });
            }

            // Mapeia parâmetros de BUTTON se existirem
            if (command.ParametrosButton != null && command.ParametrosButton.Any())
            {
                Template.Componentes.Add(new ComponenteEnvio
                {
                    Tipo = "button",
                    SubTipo = "url", // Normalmente usado para URLs dinâmicas
                    Indice = "0",    // Índice do botão no template (começa em 0)
                    Parametros = command.ParametrosButton.Select(p => new ParametroEnvio { Texto = p }).ToList()
                });
            }
        }

        [JsonProperty("messaging_product")]
        public string MessagingProduct => "whatsapp";

        [JsonProperty("to")]
        public string Para { get; set; }

        [JsonProperty("type")]
        public string Tipo => "template";

        [JsonProperty("template")]
        public TemplateData Template { get; set; }

        // Construtor que mapeia o Command para a Requisição

    }

    public class TemplateData
    {
        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("language")]
        public LanguageData Idioma { get; set; }

        [JsonProperty("components")]
        public List<ComponenteEnvio> Componentes { get; set; }
    }

    public class LanguageData
    {
        [JsonProperty("code")]
        public string Codigo { get; set; }
    }

    public class ComponenteEnvio
    {
        [JsonProperty("type")]
        public string Tipo { get; set; } // body, header, button

        [JsonProperty("sub_type", NullValueHandling = NullValueHandling.Ignore)]
        public string SubTipo { get; set; } // url, quick_reply

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public string Indice { get; set; }

        [JsonProperty("parameters")]
        public List<ParametroEnvio> Parametros { get; set; }
    }

    public class ParametroEnvio
    {
        [JsonProperty("type")]
        public string Tipo => "text";

        [JsonProperty("text")]
        public string Texto { get; set; }
    }
}
