using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosHandler : IRequestHandler<ObtemDadosBancariosCommand, Response<ObtemDadosBancariosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemDadosBancariosHandler> _logger;

        public ObtemDadosBancariosHandler(IUnitOfWork unitOfWork, ILogger<ObtemDadosBancariosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemDadosBancariosResult>> Handle(ObtemDadosBancariosCommand command)
        {
            var response = new Response<ObtemDadosBancariosResult>();

            try
            {
                var validateResult = new ObtemDadosBancariosValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                if (await _unitOfWork.Empresa.ObterPorId(command.EmpresaId) == null)
                {
                    response.AddErro("Empresa não encontrada.", 404);
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId);
                response.AddValue(new ObtemDadosBancariosResult(command.EmpresaId, dados));
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemDadosBancariosHandler));
            }

            return response;
        }
    }
}
