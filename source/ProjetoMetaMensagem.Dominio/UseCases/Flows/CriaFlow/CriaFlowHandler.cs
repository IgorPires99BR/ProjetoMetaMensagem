using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow
{
    public class CriaFlowHandler : IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CriaFlowHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaFlowResult>> Handle(CriaFlowCommand command)
        {
            var response = new Response<CriaFlowResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            Entidades.Flows flow = new Entidades.Flows(command);

            await _unitOfWork.FlowsRepository.Incluir(flow);

            response.AddValue(new CriaFlowResult());

            return response;
        }
    }
}
