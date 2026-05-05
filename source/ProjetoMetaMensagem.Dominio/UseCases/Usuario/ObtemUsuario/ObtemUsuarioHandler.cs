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
    public class ObtemUsuarioHandler : IRequestHandler<ObtemUsuarioCommand, Response<List<ObtemUsuarioResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemUsuarioHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ObtemUsuarioResult>>> Handle(ObtemUsuarioCommand command)
        {
            var response = new Response<List<ObtemUsuarioResult>>();
            var listaUsuarios = new List<ObtemUsuarioResult>();

            var validator = new ObtemUsuarioValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var usuariosBanco = await _unitOfWork.Usuario.ObterPorEmpresa(command.IdUsuario);

            foreach (var usuario in usuariosBanco)
            {
                listaUsuarios.Add(new ObtemUsuarioResult(usuario));
            }

            response.AddValue(listaUsuarios);

            return response;
        }
    }
}
