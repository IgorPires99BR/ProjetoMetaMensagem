using Microsoft.Extensions.Logging;
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

        private readonly ILogger<AlteraUsuarioHandler> _logger;

        public AlteraUsuarioHandler(IUnitOfWork unitOfWork, ILogger<AlteraUsuarioHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Response<AlteraUsuarioResult>> Handle(AlteraUsuarioCommand command)
        {
            var response = new Response<AlteraUsuarioResult>();

            try
            {
                // So admin concede admin -- mesma regra da criacao, senao a promocao seria
                // so uma edicao com "perfil": "admin" no corpo.
                if (Entidades.Usuario.EhPerfilAdmin(command.Perfil) && !command.SolicitanteEhAdmin)
                {
                    response.AddErro("Apenas um administrador pode conceder acesso de administrador.");
                    return response;
                }

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

                var linhasAfetadas = await _unitOfWork.Usuario.Alterar(usuario, command.EmpresaIdSolicitante);

                // Zero linhas: usuario inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraUsuarioResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(AlteraUsuarioHandler));
            }

            return response;
        }
    }
}


