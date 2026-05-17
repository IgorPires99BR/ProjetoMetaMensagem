using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
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

        public AtualizaTemplateMetaHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
        }

        public async Task<Response<AtualizaTemplateMetaResult>> Handle(AtualizaTemplateMetaCommand command)
        {
            var response = new Response<AtualizaTemplateMetaResult>();

            // 1. Validação do Comando
            var validator = new AtualizaTemplateMetaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // 2. Busca os templates atualizados diretamente da API da Meta
            var templatesMeta = await _metaService.ObterTemplatesMetaAsync();

            if (templatesMeta == null || !templatesMeta.Templates.Any())
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
            var nomesVindosDaMeta = templatesMeta.Templates.Select(t => t.Nome).ToList();
            var templatesParaRemover = templatesNoBanco
                .Where(b => !nomesVindosDaMeta.Contains(b.NomeTemplate))
                .ToList();

            foreach (var templateExcluir in templatesParaRemover)
            {
                await _unitOfWork.Template.Excluir(templateExcluir.Id);
            }

            var listaResultados = new List<AtualizaTemplateMetaResult>();

            // --- LÓGICA DE UPSERT (Update ou Insert) ---
            foreach (var templateApi in templatesMeta.Templates)
            {
                // Confronta usando o Nome único do Template dentro do WABA
                var templateExistente = templatesNoBanco.FirstOrDefault(x => x.NomeTemplate == templateApi.Nome);

                if (templateExistente != null)
                {
                    // --- ATUALIZAÇÃO ---
                    templateExistente.Conteudo = templateApi.ConteudoCorpo;
                    templateExistente.Categoria = templateApi.Categoria;
                    templateExistente.Idioma = templateApi.Idioma;
                    templateExistente.Status = templateApi.Status;
                    templateExistente.DataAtualizacao = DateTime.Now;

                    await _unitOfWork.Template.Alterar(templateExistente);
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
                        DataCriacao = DateTime.Now,
                        DataAtualizacao = DateTime.Now
                    };

                    await _unitOfWork.Template.Incluir(novoTemplate);
                }
            }

            return response;
        }
    }
}
