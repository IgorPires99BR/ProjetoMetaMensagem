using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemNumero
{
    public class ObtemNumeroHandler : IRequestHandler<ObtemNumeroCommand, Response<ObtemNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemNumeroHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<ObtemNumeroResult>> Handle(ObtemNumeroCommand command)
        {
            var response = new Response<ObtemNumeroResult>();

            var validator = new ObtemNumeroValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var listaEmpresa = await _unitOfWork.Empresa.Obter();

            foreach (var empresa in listaEmpresa)
            {
                response.AddValue(new ObtemNumeroResult());
            }

            return response;
        }
    }
}
