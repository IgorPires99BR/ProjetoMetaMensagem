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

                // Trocar só o nome não passa pela Meta: antes toda edição reenviava o template, o que
                // o devolvia pra análise (PENDING) e só era permitido em recusados.
                var enviarParaMeta = TemplateComponentesBuilder.AlterouConteudoEnviado(template, command.Categoria, command);

                if (enviarParaMeta)
                {
                    if (string.IsNullOrEmpty(template.Status) || !StatusEditaveis.Contains(template.Status.ToUpperInvariant()))
                    {
                        var emAnalise = string.Equals(template.Status, "PENDING", StringComparison.OrdinalIgnoreCase);

                        response.AddErro(emAnalise
                            ? "Este modelo ainda está em análise na Meta e o texto não pode ser alterado agora. Espere o resultado: se for recusado, você edita e reenvia. O nome no sistema pode ser trocado a qualquer momento."
                            : "Só é possível alterar o texto de modelos recusados pela Meta. Modelos aprovados não podem ter o texto alterado — crie um novo. O nome no sistema pode ser trocado a qualquer momento.");
                        return response;
                    }

                    if (string.IsNullOrEmpty(template.MetaTemplateId))
                    {
                        response.AddErro("Este template ainda não tem o identificador da Meta salvo localmente. Clique em \"Sincronizar Meta\" e tente editar novamente.");
                        return response;
                    }

                    var validacaoEnvio = new AtualizaTemplateEnvioMetaValidator().Validate(command);
                    if (!validacaoEnvio.IsValid)
                    {
                        response.AddErros(validacaoEnvio.Errors.ToCustomValidationFailure());
                        return response;
                    }

                    var token = await _unitOfWork.Empresa.ObterMetaAccessToken(template.EmpresaId);

                    var componentesMeta = TemplateComponentesBuilder.MontarComponentesEnvio(command);

                    await _metaService.AtualizarTemplateMetaAsync(template.MetaTemplateId, command.Categoria, componentesMeta, token);

                    template.Conteudo = command.Conteudo;
                    template.Categoria = command.Categoria;
                    template.Status = "PENDING"; // a Meta volta o template pra análise depois de uma edição
                    template.Componentes = TemplateComponentesBuilder.MontarComponentesLocais(command);
                }

                if (command.NomeExibicao != null)
                {
                    template.NomeExibicao = string.IsNullOrWhiteSpace(command.NomeExibicao) ? null : command.NomeExibicao.Trim();
                }

                template.DataAtualizacao = DateTime.Now;

                await _unitOfWork.Template.Alterar(template, command.EmpresaIdSolicitante);

                response.AddValue(new AtualizaTemplateResult(template) { EnviadoParaMeta = enviarParaMeta });
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
