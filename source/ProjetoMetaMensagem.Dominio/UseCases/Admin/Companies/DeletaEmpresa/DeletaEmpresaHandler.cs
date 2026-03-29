using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa
{
    public class DeletaEmpresaHandler : IRequestHandler<DeletaEmpresaCommand, Response<DeletaEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeletaEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaEmpresaResult>> Handle(DeletaEmpresaCommand command)
        {
            var response = new Response<DeletaEmpresaResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            await _unitOfWork.CompaniesRepository.Deletar(command.CompanyId);

            response.AddValue(new DeletaEmpresaResult());

            return response;
        }
    }
}
