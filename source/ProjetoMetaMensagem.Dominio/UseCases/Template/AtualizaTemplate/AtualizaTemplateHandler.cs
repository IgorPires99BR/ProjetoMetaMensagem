using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate
{
    public class AtualizaTemplateHandler : IRequestHandler<AtualizaTemplateCommand, Response<AtualizaTemplateResult>>
    {
        // A Meta so aceita edicao de template ja avaliado. Enquanto ele esta em analise
        // (PENDING) ela responde erro, e o usuario recebia um 500 sem explicacao nenhuma.
        private static readonly string[] StatusEditaveis = { "REJECTED", "REJECTED_META" };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<AtualizaTemplateHandler> _logger;

        public AtualizaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AtualizaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AtualizaTemplateResult>> Handle(AtualizaTemplateCommand command)
        {
            var response = new Response<AtualizaTemplateResult>();

            var validator = new AtualizaTemplateValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId, command.EmpresaIdSolicitante);

                if (template == null)
                {
                    response.AddErro("Template não encontrado.");
                    return response;
                }

                if (string.IsNullOrEmpty(template.Status) || !StatusEditaveis.Contains(template.Status.ToUpperInvariant()))
                {
                    var emAnalise = string.Equals(template.Status, "PENDING", StringComparison.OrdinalIgnoreCase);

                    response.AddErro(emAnalise
                        ? "Este modelo ainda está em análise na Meta e não pode ser alterado agora. Espere o resultado: se for recusado, você edita e reenvia."
                        : "Só é possível editar modelos recusados pela Meta. Modelos aprovados não podem ser alterados — crie um novo.");
                    return response;
                }

                if (string.IsNullOrEmpty(template.MetaTemplateId))
                {
                    response.AddErro("Este template ainda não tem o identificador da Meta salvo localmente. Clique em \"Sincronizar Meta\" e tente editar novamente.");
                    return response;
                }

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(template.EmpresaId);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(template.EmpresaId);

                var componentesMeta = TemplateComponentesBuilder.MontarComponentesEnvio(command);

                await _metaService.AtualizarTemplateMetaAsync(template.MetaTemplateId, command.Categoria, componentesMeta, token);

                template.Conteudo = command.Conteudo;
                template.Categoria = command.Categoria;
                template.Status = "PENDING"; // a Meta volta o template pra análise depois de uma edição
                template.DataAtualizacao = DateTime.Now;
                template.Componentes = TemplateComponentesBuilder.MontarComponentesLocais(command);

                await _unitOfWork.Template.Alterar(template, command.EmpresaIdSolicitante);

                response.AddValue(new AtualizaTemplateResult(template));
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(AtualizaTemplateHandler));
            }

            return response;
        }
    }
}
