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

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraUsuarioValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Preserva o hash existente quando a senha e deixada em branco na edicao
                // (o UPDATE sempre sobrescreve SenhaHash, entao sem isso o campo era
                // zerado toda vez que o usuario era editado sem trocar a senha).
                var usuarioExistente = await _unitOfWork.Usuario.ObterPorId(command.Id);
                if (usuarioExistente == null)
                {
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                var usuario = new Entidades.Usuario(command)
                {
                    SenhaHash = !string.IsNullOrEmpty(command.SenhaHash)
                        ? BCrypt.Net.BCrypt.HashPassword(command.SenhaHash)
                        : usuarioExistente.SenhaHash
                };

                await _unitOfWork.Usuario.Alterar(usuario);

                response.AddValue(new AlteraUsuarioResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


