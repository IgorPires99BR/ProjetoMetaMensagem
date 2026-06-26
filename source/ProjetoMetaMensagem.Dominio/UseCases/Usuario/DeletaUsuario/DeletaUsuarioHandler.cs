using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario
{
    public class DeletaUsuarioHandler : IRequestHandler<DeletaUsuarioCommand, Response<DeletaUsuarioResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaUsuarioHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaUsuarioResult>> Handle(DeletaUsuarioCommand command)
        {
            var response = new Response<DeletaUsuarioResult>();

            try
            {
                var validator = new DeletaUsuarioValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                await _unitOfWork.Usuario.Excluir(command.Id.ToString());

                response.AddValue(new DeletaUsuarioResult());
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
