using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario
{
    public class CriaUsuarioHandler : IRequestHandler<CriaUsuarioCommand, Response<CriaUsuarioResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriaUsuarioHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Response<CriaUsuarioResult>> Handle(CriaUsuarioCommand command)
        {
            var response = new Response<CriaUsuarioResult>();

            var validator = new CriaUsuarioValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            await _unitOfWork.Usuario.Incluir(new Entidades.Usuario(command));

            response.AddValue(new CriaUsuarioResult());

            return response;
        }
    }
}
