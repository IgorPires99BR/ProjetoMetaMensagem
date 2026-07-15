using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.Helpers;
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

        public EnviarMensagemTemplateRequisicao(EnviarMensagemTemplateMetaCommand command, string contatoId)
        {
            if (command == null) return;

            Para = TelefoneHelper.FormatarParaMeta(command.Telefone);
            ContatoId = contatoId;
            Template = new TemplateData
            {
                Nome = command.NomeTemplate,
                Idioma = new LanguageData { Codigo = command.Idioma },
                Componentes = new List<ComponenteEnvio>()
            };

            // 1. Mapeia parâmetros do HEADER de mídia se existirem no Command (Dinâmico por extensão de arquivo)
            if (!string.IsNullOrEmpty(command.ParametroHeaderMediaUrl))
            {
                string tipoMidia = "image";
                string urlLower = command.ParametroHeaderMediaUrl.ToLower();

                if (urlLower.EndsWith(".pdf") || urlLower.EndsWith(".doc") || urlLower.EndsWith(".docx"))
                    tipoMidia = "document";
                else if (urlLower.EndsWith(".mp4") || urlLower.EndsWith(".avi") || urlLower.EndsWith(".mov"))
                    tipoMidia = "video";

                var parametroHeader = new ParametroEnvio
                {
                    Tipo = tipoMidia
                };

                // Atribui ao nó correto esperado pela API da Meta
                if (tipoMidia == "image")
                    parametroHeader.Image = new MediaLinkData { Link = command.ParametroHeaderMediaUrl };
                else if (tipoMidia == "document")
                    parametroHeader.Document = new MediaLinkData { Link = command.ParametroHeaderMediaUrl };
                else if (tipoMidia == "video")
                    parametroHeader.Video = new MediaLinkData { Link = command.ParametroHeaderMediaUrl };

                Template.Componentes.Add(new ComponenteEnvio
                {
                    Tipo = "header",
                    Parametros = new List<ParametroEnvio> { parametroHeader }
                });
            }

            // 2. Mapeia parâmetros do BODY se existirem
            if (command.ParametrosBody != null && command.ParametrosBody.Any())
            {
                Template.Componentes.Add(new ComponenteEnvio
                {
                    Tipo = "body",
                    Parametros = command.ParametrosBody.Select(p => new ParametroEnvio
                    {
                        Tipo = "text",
                        Texto = p
                    }).ToList()
                });
            }

            // 3. Mapeia parâmetros de BUTTON se existirem
            if (command.ParametrosButton != null && command.ParametrosButton.Any(p => !string.IsNullOrWhiteSpace(p)))
            {
                Template.Componentes.Add(new ComponenteEnvio
                {
                    Tipo = "button",
                    SubTipo = "url",
                    Indice = "0",
                    Parametros = command.ParametrosButton
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => new ParametroEnvio
                        {
                            Tipo = "text",
                            Texto = p
                        }).ToList()
                });
            }
        }

        // CORREÇÃO: Ignora esta propriedade no JSON enviado para a Meta, mantendo-a viva no seu ecossistema C#
        [JsonIgnore]
        public string ContatoId { get; set; }

        [JsonProperty("messaging_product")]
        public string MessagingProduct => "whatsapp";

        [JsonProperty("to")]
        public string Para { get; set; }

        [JsonProperty("type")]
        public string Tipo => "template";

        [JsonProperty("template")]
        public TemplateData Template { get; set; }
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
        public string Tipo { get; set; }

        [JsonProperty("sub_type", NullValueHandling = NullValueHandling.Ignore)]
        public string SubTipo { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public string Indice { get; set; }

        [JsonProperty("parameters")]
        public List<ParametroEnvio> Parametros { get; set; }
    }

    public class ParametroEnvio
    {
        [JsonProperty("type")] // Newtonsoft
        [System.Text.Json.Serialization.JsonPropertyName("type")] // Nativo .NET
        public string Tipo { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string Texto { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("image")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public MediaLinkData Image { get; set; }

        [JsonProperty("document", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("document")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public MediaLinkData Document { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("video")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public MediaLinkData Video { get; set; }
    }

    public class MediaLinkData
    {
        [JsonProperty("link")]
        [System.Text.Json.Serialization.JsonPropertyName("link")] // Garante o "link" minúsculo no .NET nativo
        public string Link { get; set; }
    }
}