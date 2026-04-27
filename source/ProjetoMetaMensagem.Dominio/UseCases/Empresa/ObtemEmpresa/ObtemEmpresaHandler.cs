using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    public class ObtemEmpresaHandler : IRequestHandler<ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<List<ObtemEmpresaResult>>> Handle(ObtemEmpresaCommand command)
        {
            var response = new Response<List<ObtemEmpresaResult>>();
            var listaEmpresa = new List<ObtemEmpresaResult>();

            // Validação: criar e usar um validador específico (AlteraEmpresaValidator) similar ao CriaEmpresaValidator
            var validator = new ObtemEmpresaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var empresaBanco = await _unitOfWork.Empresa.Obter();

            foreach(var empresa in empresaBanco)
            {
                listaEmpresa.Add(new ObtemEmpresaResult(empresa));
            }

            response.AddValue(listaEmpresa);

            return response;
        }
    }
}
