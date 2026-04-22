using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioHandler : IRequestHandler<ObtemUsuarioCommand, Response<ObtemUsuarioResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemUsuarioHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ObtemUsuarioResult>> Handle(ObtemUsuarioCommand command)
        {
            var response = new Response<ObtemUsuarioResult>();

            var validator = new ObtemUsuarioValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var listaEmpresa = await _unitOfWork.Empresa.Obter();

            foreach (var empresa in listaEmpresa)
            {
                response.AddValue(new ObtemUsuarioResult());
            }

            return response;
        }
    }
}
