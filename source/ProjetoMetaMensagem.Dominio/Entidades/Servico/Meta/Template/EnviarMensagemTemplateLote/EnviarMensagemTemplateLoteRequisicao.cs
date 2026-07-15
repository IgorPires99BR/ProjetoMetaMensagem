using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Dominio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote
{
    public class EnviarMensagemTemplateLoteRequisicao
    {
        public List<string> Telefones { get; set; } = new List<string>();
        public List<string> ContatosIds { get; set; } = new List<string>();
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; }
        public string? ParametroHeaderMediaUrl { get; set; }
        public List<string> ParametrosBody { get; set; }
        public List<string> ParametrosButton { get; set; }

        public EnviarMensagemTemplateLoteRequisicao()
        {
        }

        // Construtor que mapeia o Command em lote para a estrutura da requisição
        public EnviarMensagemTemplateLoteRequisicao(EnviarMensagemTemplateMetaLoteCommand command)
        {
            if (command == null) return;

            Telefones = command.Telefones ?? new List<string>();
            ContatosIds = command.ContatosIds ?? new List<string>();
            NomeTemplate = command.NomeTemplate;
            Idioma = command.Idioma;
            ParametroHeaderMediaUrl = command.ParametroHeaderMediaUrl;
            ParametrosBody = command.ParametrosBody;
            ParametrosButton = command.ParametrosButton;
        }

        /// <summary>
        /// Explode o lote em uma lista de requisições individuais prontas para a API da Meta
        /// </summary>
        public List<EnviarMensagemTemplateRequisicao> GerarRequisicoesIndividuais()
        {
            var requisicoes = new List<EnviarMensagemTemplateRequisicao>();
            int index = 0;

            foreach (var telefone in Telefones)
            {
                string contatoId = (ContatosIds != null && ContatosIds.Count > index)
                 ? ContatosIds[index]
                 : "00000000-0000-0000-0000-000000000000";

                // Instancia o objeto individual com telefone formatado
                var reqIndividual = new EnviarMensagemTemplateRequisicao
                {
                    Para = TelefoneHelper.FormatarParaMeta(telefone),
                    ContatoId = contatoId,
                    Template = new TemplateData
                    {
                        Nome = this.NomeTemplate,
                        Idioma = new LanguageData { Codigo = this.Idioma },
                        Componentes = new List<ComponenteEnvio>()
                    }
                };

                // 1. CORREÇÃO DO HEADER: Mapeia mídias utilizando o tipo MediaLinkData fortemente tipado
                if (!string.IsNullOrEmpty(this.ParametroHeaderMediaUrl))
                {
                    string tipoMidia = "image";
                    string urlLower = this.ParametroHeaderMediaUrl.ToLower();

                    if (urlLower.EndsWith(".pdf") || urlLower.EndsWith(".doc") || urlLower.EndsWith(".docx"))
                        tipoMidia = "document";
                    else if (urlLower.EndsWith(".mp4") || urlLower.EndsWith(".avi") || urlLower.EndsWith(".mov"))
                        tipoMidia = "video";

                    var parametroMidia = new ParametroEnvio
                    {
                        Tipo = tipoMidia
                    };

                    // Injeta a URL no nó correto baseado no tipo detectado, utilizando a classe interna MediaLinkData
                    if (tipoMidia == "image")
                        parametroMidia.Image = new MediaLinkData { Link = this.ParametroHeaderMediaUrl };
                    else if (tipoMidia == "document")
                        parametroMidia.Document = new MediaLinkData { Link = this.ParametroHeaderMediaUrl };
                    else if (tipoMidia == "video")
                        parametroMidia.Video = new MediaLinkData { Link = this.ParametroHeaderMediaUrl };

                    reqIndividual.Template.Componentes.Add(new ComponenteEnvio
                    {
                        Tipo = "header",
                        Parametros = new List<ParametroEnvio> { parametroMidia }
                    });
                }

                // 2. Mapeia parâmetros do BODY se existirem
                if (this.ParametrosBody != null && this.ParametrosBody.Any())
                {
                    reqIndividual.Template.Componentes.Add(new ComponenteEnvio
                    {
                        Tipo = "body",
                        Parametros = this.ParametrosBody.Select(p => new ParametroEnvio
                        {
                            Tipo = "text",
                            Texto = p
                        }).ToList()
                    });
                }

                // 3. Mapeia parâmetros do BUTTON se existirem
                if (this.ParametrosButton != null && this.ParametrosButton.Any(p => !string.IsNullOrWhiteSpace(p)))
                {
                    reqIndividual.Template.Componentes.Add(new ComponenteEnvio
                    {
                        Tipo = "button",
                        SubTipo = "url",
                        Indice = "0",
                        Parametros = this.ParametrosButton
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .Select(p => new ParametroEnvio
                            {
                                Tipo = "text",
                                Texto = p
                            }).ToList()
                    });
                }

                requisicoes.Add(reqIndividual);
                index++;
            }

            return requisicoes;
        }
    }
}