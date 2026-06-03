using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
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
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; }
        public List<string> ParametrosBody { get; set; }
        public List<string> ParametrosButton { get; set; }

        // Construtor que mapeia o Command em lote para a estrutura da requisição
        public EnviarMensagemTemplateLoteRequisicao(EnviarMensagemTemplateMetaLoteCommand command)
        {
            if (command == null) return;

            Telefones = command.Telefones ?? new List<string>();
            NomeTemplate = command.NomeTemplate;
            Idioma = command.Idioma;
            ParametrosBody = command.ParametrosBody;
            ParametrosButton = command.ParametrosButton;
        }

        /// <summary>
        /// Explode o lote em uma lista de requisições individuais prontas para a API da Meta
        /// </summary>
        public List<EnviarMensagemTemplateRequisicao> GerarRequisicoesIndividuais()
        {
            var requisicoes = new List<EnviarMensagemTemplateRequisicao>();

            foreach (var telefone in Telefones)
            {
                // Instancia o objeto individual
                var reqIndividual = new EnviarMensagemTemplateRequisicao
                {
                    Para = telefone,
                    Template = new TemplateData
                    {
                        Nome = this.NomeTemplate,
                        Idioma = new LanguageData { Codigo = this.Idioma },
                        Componentes = new List<ComponenteEnvio>()
                    }
                };

                // Mapeia parâmetros do BODY se existiren
                if (this.ParametrosBody != null && this.ParametrosBody.Any())
                {
                    reqIndividual.Template.Componentes.Add(new ComponenteEnvio
                    {
                        Tipo = "body",
                        Parametros = this.ParametrosBody.Select(p => new ParametroEnvio { Texto = p }).ToList()
                    });
                }

                // Mapeia parâmetros de BUTTON se existirem
                if (this.ParametrosButton != null && this.ParametrosButton.Any())
                {
                    reqIndividual.Template.Componentes.Add(new ComponenteEnvio
                    {
                        Tipo = "button",
                        SubTipo = "url",
                        Indice = "0",
                        Parametros = this.ParametrosButton.Select(p => new ParametroEnvio { Texto = p }).ToList()
                    });
                }

                requisicoes.Add(reqIndividual);
            }

            return requisicoes;
        }
    }
}
