using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta
{
    public class AtualizaTemplateMetaHandler : IRequestHandler<AtualizaTemplateMetaCommand, Response<AtualizaTemplateMetaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<AtualizaTemplateMetaHandler> _logger;

        public AtualizaTemplateMetaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AtualizaTemplateMetaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AtualizaTemplateMetaResult>> Handle(AtualizaTemplateMetaCommand command)
        {
            var response = new Response<AtualizaTemplateMetaResult>();

            try
            {
                // 1. Validação do Comando
                var validator = new AtualizaTemplateMetaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // 2. Busca os templates atualizados diretamente da API da Meta
                var templatesMeta = await _metaService.ObterTemplatesMetaAsync(wabaId, token);

                if (templatesMeta == null || !templatesMeta.Any())
                {
                    response.AddErro("Nenhum template encontrado na API da Meta para esta conta.");
                    return response;
                }

                // 3. Busca todos os templates já gravados no banco para a Empresa do comando
                // Como mapeado no banco, templates pertencem à EmpresaId
                var templatesNoBanco = await _unitOfWork.Template.ObterPorEmpresa(command.IdEmpresa);

                // --- LÓGICA DE EXCLUSÃO (Sincronização de órfãos) ---
                // Identifica templates que estão no banco, mas foram deletados no painel da Meta
                // Nota: Se você não usar o Id da Meta como PK física, adapte o NomeTemplate ou crie um campo 'MetaTemplateId'
                var nomesVindosDaMeta = templatesMeta.Select(t => t.Nome).ToList();
                var templatesParaRemover = templatesNoBanco
                    .Where(b => !nomesVindosDaMeta.Contains(b.NomeTemplate))
                    .ToList();

                foreach (var templateExcluir in templatesParaRemover)
                {
                    // Sincronizacao sempre restrita a empresa do comando, mesmo que a lista
                    // de templates ja tenha vindo dela: assim um id de outra empresa que
                    // escape pra ca nao chega a ser apagado.
                    await _unitOfWork.Template.Excluir(templateExcluir.Id, command.IdEmpresa);
                }

                var listaResultados = new List<AtualizaTemplateMetaResult>();

                // --- LÓGICA DE UPSERT (Update ou Insert) ---
                foreach (var templateApi in templatesMeta)
                {
                    // Confronta preferencialmente pelo MetaTemplateId (estavel mesmo se o nome mudar
                    // de outra forma); templates legados sincronizados antes desse campo existir caem
                    // no fallback por NomeTemplate.
                    var templateExistente = templatesNoBanco.FirstOrDefault(x => x.MetaTemplateId == templateApi.Id)
                        ?? templatesNoBanco.FirstOrDefault(x => x.NomeTemplate == templateApi.Nome);

                    // Converte do formato de leitura da Meta para o formato persistido localmente
                    var componentesPersistidos = MapearParaPersistencia(templateApi.Componentes);

                    if (templateExistente != null)
                    {
                        // --- ATUALIZAÇÃO ---
                        templateExistente.Conteudo = templateApi.ConteudoCorpo;
                        templateExistente.Categoria = templateApi.Categoria;
                        templateExistente.Idioma = templateApi.Idioma;
                        templateExistente.Status = templateApi.Status;
                        templateExistente.MetaTemplateId = templateApi.Id;
                        templateExistente.DataAtualizacao = DateTime.Now;

                        // Atualiza a árvore simplificada de componentes (O setter cuida da serialização JSON)
                        templateExistente.Componentes = componentesPersistidos;

                        await _unitOfWork.Template.Alterar(templateExistente, command.IdEmpresa);
                    }
                    else
                    {
                        // --- INSERÇÃO ---
                        var novoTemplate = new Entidades.Template
                        {
                            Id = Guid.NewGuid(),
                            EmpresaId = command.IdEmpresa,
                            NomeTemplate = templateApi.Nome,
                            Conteudo = templateApi.ConteudoCorpo,
                            Categoria = templateApi.Categoria,
                            Idioma = templateApi.Idioma,
                            Status = templateApi.Status,
                            MetaTemplateId = templateApi.Id,
                            Componentes = componentesPersistidos,
                            DataCriacao = DateTime.Now,
                            DataAtualizacao = DateTime.Now
                        };

                        await _unitOfWork.Template.Incluir(novoTemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AtualizaTemplateMetaHandler));
            }

            return response;
        }

        // Converte o formato de leitura da Meta (ComponenteTemplateMetaDto) para o formato
        // persistido localmente (TemplateComponenteDto) -- são tipos deliberadamente distintos.
        private static List<TemplateComponenteDto> MapearParaPersistencia(List<ComponenteTemplateMetaDto> componentesMeta)
        {
            if (componentesMeta == null) return new List<TemplateComponenteDto>();

            return componentesMeta.Select(c => new TemplateComponenteDto
            {
                Tipo = c.Tipo,
                FormatMidia = c.FormatMidia,
                Texto = c.Texto,
                Botoes = c.Botoes?.Select(b => new TemplateBotaoDto
                {
                    Tipo = b.Tipo,
                    Texto = b.Texto,
                    Url = b.Url,
                    NumeroTelefone = b.NumeroTelefone,
                    CodigoExemplo = b.CodigoExemplo
                }).ToList()
            }).ToList();
        }
    }
}
