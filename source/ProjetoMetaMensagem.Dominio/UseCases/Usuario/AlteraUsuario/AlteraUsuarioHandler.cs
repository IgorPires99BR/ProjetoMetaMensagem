using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario
{
    public class AlteraUsuarioHandler : IRequestHandler<AlteraUsuarioCommand, Response<AlteraUsuarioResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlteraUsuarioHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Response<AlteraUsuarioResult>> Handle(AlteraUsuarioCommand command)
        {
            var response = new Response<AlteraUsuarioResult>();

            var validator = new AlteraUsuarioValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            await _unitOfWork.Usuario.Alterar(new Entidades.Usuario(command));

            response.AddValue(new AlteraUsuarioResult());

            return response;
        }
    }
}
