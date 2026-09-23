using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraFlagCobrancaTemplate
{
    public class AlteraFlagCobrancaTemplateHandler : IRequestHandler<AlteraFlagCobrancaTemplateCommand, Response<AlteraFlagCobrancaTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AlteraFlagCobrancaTemplateHandler> _logger;

        public AlteraFlagCobrancaTemplateHandler(IUnitOfWork unitOfWork, ILogger<AlteraFlagCobrancaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraFlagCobrancaTemplateResult>> Handle(AlteraFlagCobrancaTemplateCommand command)
        {
            var response = new Response<AlteraFlagCobrancaTemplateResult>();

            var validator = new AlteraFlagCobrancaTemplateValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId, command.EmpresaIdSolicitante);

                if (template == null)
                {
                    response.AddErro("Template não encontrado.");
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Template.AlterarFlagCobranca(
                    command.TemplateId, command.EmpresaIdSolicitante, command.GeraCobranca);

                if (linhasAfetadas == 0)
                {
                    response.AddErro("Template não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraFlagCobrancaTemplateResult
                {
                    TemplateId = command.TemplateId,
                    GeraCobranca = command.GeraCobranca
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraFlagCobrancaTemplateHandler));
            }

            return response;
        }
    }
}
