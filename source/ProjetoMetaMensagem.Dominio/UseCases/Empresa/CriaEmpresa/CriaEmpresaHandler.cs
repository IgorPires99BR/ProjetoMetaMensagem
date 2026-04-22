using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa
{
    public class CriaEmpresaHandler : IRequestHandler<CriaEmpresaCommand, Response<CriaEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CriaEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaEmpresaResult>> Handle(CriaEmpresaCommand command)
        {
            var response = new Response<CriaEmpresaResult>();

            var validator = new CriaEmpresaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            await _unitOfWork.Empresa.Incluir(new Entidades.Empresa(command));

            response.AddValue(new CriaEmpresaResult());

            return response;
        }
    }
}
