using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.Common
{
    // Monta os componentes de template (Header/Body/Footer/Buttons) a partir do comando de
    // entrada, tanto no formato esperado pela Meta (envio) quanto no formato persistido
    // localmente. Usado por CriaTemplateHandler e AtualizaTemplateHandler para não duplicar
    // essa montagem.
    public static class TemplateComponentesBuilder
    {
        public static bool TemHeader(ITemplateComponentesInput input) =>
            !string.IsNullOrEmpty(input.HeaderTipo) && input.HeaderTipo != "NONE";

        public static bool TemFooter(ITemplateComponentesInput input) =>
            !string.IsNullOrWhiteSpace(input.FooterTexto);

        public static bool TemBotoes(ITemplateComponentesInput input) =>
            input.Botoes != null && input.Botoes.Any();

        public static List<ComponenteTemplateEnvio> MontarComponentesEnvio(ITemplateComponentesInput input)
        {
            var componentes = new List<ComponenteTemplateEnvio>();
            var quantidadeVariaveis = Regex.Matches(input.Conteudo ?? string.Empty, @"\{\{\d+\}\}").Count;

            if (TemHeader(input))
            {
                var headerMeta = new ComponenteTemplateEnvio
                {
                    Tipo = "HEADER",
                    Formato = input.HeaderTipo
                };

                if (input.HeaderTipo == "TEXT")
                {
                    headerMeta.Texto = input.HeaderTexto;
                }
                else
                {
                    // IMAGE/VIDEO/DOCUMENT: a Meta exige o handle do upload prévio (Resumable Upload API), não aceita URL direta
                    headerMeta.HeaderHandle = new List<string> { input.HeaderExemploHandle };
                }

                componentes.Add(headerMeta);
            }

            var bodyMeta = new ComponenteTemplateEnvio
            {
                Tipo = "BODY",
                Texto = input.Conteudo
            };

            if (quantidadeVariaveis > 0)
            {
                // A Meta exige um valor de exemplo por variável do corpo
                bodyMeta.BodyTextExemplos = new List<List<string>> { input.ExemplosBody };
            }

            componentes.Add(bodyMeta);

            if (TemFooter(input))
            {
                componentes.Add(new ComponenteTemplateEnvio
                {
                    Tipo = "FOOTER",
                    Texto = input.FooterTexto
                });
            }

            if (TemBotoes(input))
            {
                componentes.Add(new ComponenteTemplateEnvio
                {
                    Tipo = "BUTTONS",
                    Botoes = input.Botoes.Select(b => new BotaoTemplateEnvio
                    {
                        Tipo = b.Tipo,
                        Texto = b.Texto,
                        Url = b.Url,
                        NumeroTelefone = b.NumeroTelefone,
                        CodigoExemplo = b.CodigoExemplo
                    }).ToList()
                });
            }

            return componentes;
        }

        public static List<TemplateComponenteDto> MontarComponentesLocais(ITemplateComponentesInput input)
        {
            var componentesLocais = new List<TemplateComponenteDto>();

            if (TemHeader(input))
            {
                componentesLocais.Add(new TemplateComponenteDto
                {
                    Tipo = TipoComponenteTemplate.Header,
                    Texto = input.HeaderTipo == "TEXT" ? input.HeaderTexto : null,
                    FormatMidia = input.HeaderTipo switch
                    {
                        "TEXT" => TipoMidiaTemplate.Text,
                        "IMAGE" => TipoMidiaTemplate.Image,
                        "VIDEO" => TipoMidiaTemplate.Video,
                        "DOCUMENT" => TipoMidiaTemplate.Document,
                        _ => TipoMidiaTemplate.None
                    }
                });
            }

            if (TemFooter(input))
            {
                componentesLocais.Add(new TemplateComponenteDto
                {
                    Tipo = TipoComponenteTemplate.Footer,
                    Texto = input.FooterTexto
                });
            }

            if (TemBotoes(input))
            {
                componentesLocais.Add(new TemplateComponenteDto
                {
                    Tipo = TipoComponenteTemplate.Buttons,
                    Botoes = input.Botoes.Select(b => new TemplateBotaoDto
                    {
                        Tipo = b.Tipo switch
                        {
                            "URL" => TipoBotaoTemplate.Url,
                            "PHONE_NUMBER" => TipoBotaoTemplate.PhoneNumber,
                            "COPY_CODE" => TipoBotaoTemplate.CopyCode,
                            _ => TipoBotaoTemplate.QuickReply
                        },
                        Texto = b.Texto,
                        Url = b.Url,
                        NumeroTelefone = b.NumeroTelefone,
                        CodigoExemplo = b.CodigoExemplo
                    }).ToList()
                });
            }

            return componentesLocais;
        }
    }
}
