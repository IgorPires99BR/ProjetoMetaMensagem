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
    public class ObtemNumeroHandler : IRequestHandler<ObtemNumeroCommand, Response<List<ObtemNumeroResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemNumeroHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<List<ObtemNumeroResult>>> Handle(ObtemNumeroCommand command)
        {
            var response = new Response<List<ObtemNumeroResult>>();
            var listaNumeros = new List<ObtemNumeroResult>();

            var validator = new ObtemNumeroValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var listaNumeroBanco = await _unitOfWork.Numero.ObterPorUsuario(command.IdEmpresa);

            foreach (var numero in listaNumeroBanco)
            {
                listaNumeros.Add(new ObtemNumeroResult(numero));
            }

            response.AddValue(listaNumeros);

            return response;
        }
    }
}
