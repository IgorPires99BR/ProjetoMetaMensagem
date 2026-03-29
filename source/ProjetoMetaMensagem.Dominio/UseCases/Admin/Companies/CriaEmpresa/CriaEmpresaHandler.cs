using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa
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

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            Entidades.Companies company = new Entidades.Companies(command);

            await _unitOfWork.CompaniesRepository.Incluir(company);

            response.AddValue(new CriaEmpresaResult());

            return response;
        }
    }
}
