using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.ObtemTemplateMeta;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateHandler : IRequestHandler<CriaTemplateCommand, Response<CriaTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<CriaTemplateHandler> _logger;

        public CriaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<CriaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<CriaTemplateResult>> Handle(CriaTemplateCommand command)
        {
            var response = new Response<CriaTemplateResult>();

            // 1. Validação do Comando (Entrada do Request do Mediator)
            var validator = new CriaTemplateValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 2. Monta o payload estruturado que o 'CriarTemplateMetaAsync' do seu MetaService espera
                var temHeader = !string.IsNullOrEmpty(command.HeaderTipo) && command.HeaderTipo != "NONE";
                var quantidadeVariaveis = Regex.Matches(command.Conteudo ?? string.Empty, @"\{\{\d+\}\}").Count;

                var componentesMeta = new List<ComponenteTemplate>();

                if (temHeader)
                {
                    var headerMeta = new ComponenteTemplate
                    {
                        Tipo = "HEADER",
                        Formato = command.HeaderTipo
                    };

                    if (command.HeaderTipo == "TEXT")
                    {
                        headerMeta.Texto = command.HeaderTexto;
                    }
                    else
                    {
                        // IMAGE/VIDEO/DOCUMENT: a Meta exige o handle do upload prévio (Resumable Upload API), não aceita URL direta
                        headerMeta.Example = new TemplateExample { HeaderHandle = new List<string> { command.HeaderExemploHandle } };
                    }

                    componentesMeta.Add(headerMeta);
                }

                var bodyMeta = new ComponenteTemplate
                {
                    Tipo = "BODY",
                    Texto = command.Conteudo
                };

                if (quantidadeVariaveis > 0)
                {
                    // A Meta exige um valor de exemplo por variável do corpo
                    bodyMeta.Example = new TemplateExample { BodyText = new List<List<string>> { command.ExemplosBody } };
                }

                componentesMeta.Add(bodyMeta);

                var temBotoes = command.Botoes != null && command.Botoes.Any();

                if (temBotoes)
                {
                    componentesMeta.Add(new ComponenteTemplate
                    {
                        Tipo = "BUTTONS",
                        Botoes = command.Botoes.Select(b => new BotaoTemplate
                        {
                            Tipo = b.Tipo,
                            Texto = b.Texto,
                            Url = b.Url,
                            NumeroTelefone = b.NumeroTelefone
                        }).ToList()
                    });
                }

                var requisicaoMeta = new CreateTemplateRequisicao
                {
                    Nome = command.NomeTemplate,
                    Categoria = command.Categoria,
                    Idioma = command.Idioma ?? "pt_BR",
                    // A Meta exige os componentes separados (HEADER, BODY, BUTTONS).
                    Componentes = componentesMeta
                };

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);


                // 3. Dispara a criação para a API da Meta
                // Esse método retorna a string JSON de resposta contendo o ID gerado pela Meta
                var respostaMetaJson = await _metaService.CriarTemplateMetaAsync(requisicaoMeta, wabaId, token);

                if (string.IsNullOrEmpty(respostaMetaJson))
                {
                    response.AddErro("A Meta aceitou o template, mas retornou uma resposta vazia.");
                    return response;
                }

                // 4. Cria a entidade de Domínio para salvar no banco local
                // Nota: Por padrão, todo template recém-criado entra em análise na Meta com o status "PENDING"
                var novoTemplate = new Entidades.Template
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = command.IdEmpresa,
                    NomeTemplate = command.NomeTemplate,
                    Conteudo = command.Conteudo,
                    Categoria = command.Categoria,
                    Idioma = command.Idioma ?? "pt_BR",
                    Status = "PENDING",
                    DataCriacao = DateTime.Now
                };

                var componentesLocais = new List<TemplateComponenteDto>();

                if (temHeader)
                {
                    componentesLocais.Add(new TemplateComponenteDto
                    {
                        Tipo = TipoComponenteTemplate.Header,
                        Texto = command.HeaderTipo == "TEXT" ? command.HeaderTexto : null,
                        FormatMidia = command.HeaderTipo switch
                        {
                            "TEXT" => TipoMidiaTemplate.Text,
                            "IMAGE" => TipoMidiaTemplate.Image,
                            "VIDEO" => TipoMidiaTemplate.Video,
                            "DOCUMENT" => TipoMidiaTemplate.Document,
                            _ => TipoMidiaTemplate.None
                        }
                    });
                }

                if (temBotoes)
                {
                    componentesLocais.Add(new TemplateComponenteDto
                    {
                        Tipo = TipoComponenteTemplate.Buttons,
                        Botoes = command.Botoes.Select(b => new TemplateBotaoDto
                        {
                            Tipo = b.Tipo switch
                            {
                                "URL" => TipoBotaoTemplate.Url,
                                "PHONE_NUMBER" => TipoBotaoTemplate.PhoneNumber,
                                _ => TipoBotaoTemplate.QuickReply
                            },
                            Texto = b.Texto,
                            Url = b.Url
                        }).ToList()
                    });
                }

                if (componentesLocais.Any())
                {
                    novoTemplate.Componentes = componentesLocais;
                }

                // 5. Persiste as alterações via Unit of Work
                await _unitOfWork.Template.Incluir(novoTemplate);

                // 6. Retorna o resultado mapeado para o padrão do seu Use Case
                response.AddValue(new CriaTemplateResult(novoTemplate));
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Captura e formata erros de HttpClient da Meta ou falhas no banco local
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(CriaTemplateHandler)));
            }

            return response;
        }
    }
}


