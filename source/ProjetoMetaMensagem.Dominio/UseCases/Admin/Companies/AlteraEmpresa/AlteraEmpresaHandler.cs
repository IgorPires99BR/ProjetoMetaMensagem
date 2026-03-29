using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa
{
    public class AlteraEmpresaHandler : IRequestHandler<AlteraEmpresaCommand, Response<AlteraEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AlteraEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AlteraEmpresaResult>> Handle(AlteraEmpresaCommand command)
        {
            var response = new Response<AlteraEmpresaResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            Entidades.Companies company = new Entidades.Companies(command);

            await _unitOfWork.CompaniesRepository.Alterar(command.Id, company);

            response.AddValue(new AlteraEmpresaResult());

            return response;
        }
    }
}
