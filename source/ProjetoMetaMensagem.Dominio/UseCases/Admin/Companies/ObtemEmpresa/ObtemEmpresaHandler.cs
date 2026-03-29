using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa
{
    public class ObtemEmpresaHandler : IRequestHandler<ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObtemEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ObtemEmpresaResult>>> Handle(ObtemEmpresaCommand request)
        {
            var response = new Response<List<ObtemEmpresaResult>>();

            var listaResultados = new List<ObtemEmpresaResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            var companies = await _unitOfWork.CompaniesRepository.Obter();


            foreach (var compania in companies)
            {
                listaResultados.Add(new ObtemEmpresaResult(compania));
            }

            response.AddValue(listaResultados);

            return response;
        }
    }
}
