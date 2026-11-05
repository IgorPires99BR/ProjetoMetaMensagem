using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraAtivoTemplate
{
    public class AlteraAtivoTemplateHandler : IRequestHandler<AlteraAtivoTemplateCommand, Response<AlteraAtivoTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AlteraAtivoTemplateHandler> _logger;

        public AlteraAtivoTemplateHandler(IUnitOfWork unitOfWork, ILogger<AlteraAtivoTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraAtivoTemplateResult>> Handle(AlteraAtivoTemplateCommand command)
        {
            var response = new Response<AlteraAtivoTemplateResult>();

            var validateResult = new AlteraAtivoTemplateValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var linhasAfetadas = await _unitOfWork.Template.AlterarAtivo(command.TemplateId, command.EmpresaIdSolicitante, command.Ativo);
                if (linhasAfetadas == 0)
                {
                    response.AddErro(command.Ativo ? "Modelo não encontrado ou já está ativo." : "Modelo não encontrado ou já está inativo.");
                    return response;
                }

                response.AddValue(new AlteraAtivoTemplateResult { TemplateId = command.TemplateId, Ativo = command.Ativo });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraAtivoTemplateHandler));
            }

            return response;
        }
    }
}
