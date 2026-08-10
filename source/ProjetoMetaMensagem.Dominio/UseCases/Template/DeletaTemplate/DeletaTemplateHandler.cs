using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateHandler : IRequestHandler<DeletaTemplateCommand, Response<DeletaTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<DeletaTemplateHandler> _logger;

        public DeletaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<DeletaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<DeletaTemplateResult>> Handle(DeletaTemplateCommand request)
        {
            var response = new Response<DeletaTemplateResult>();

            var validator = new DeletaTemplateValidator();
            var validateResult = validator.Validate(request);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(request.TemplateId, request.EmpresaIdSolicitante);

                if (template == null)
                {
                    response.AddErro("Template não encontrado.");
                    return response;
                }

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(template.EmpresaId);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(template.EmpresaId);

                await _metaService.ExcluirTemplateMetaAsync(template.NomeTemplate, template.MetaTemplateId, wabaId, token);

                await _unitOfWork.Template.Excluir(template.Id, request.EmpresaIdSolicitante);

                response.AddValue(new DeletaTemplateResult(template));
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(DeletaTemplateHandler));
            }

            return response;
        }
    }
}
