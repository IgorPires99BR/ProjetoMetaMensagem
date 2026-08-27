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

            try
            {
                // Empresa vem do escopo do token (null so pra conta de plataforma, que pode
                // criar usuario em qualquer empresa). Se viesse do corpo, um usuario de uma
                // empresa criaria usuarios dentro de outra.
                if (command.EmpresaIdSolicitante.HasValue)
                {
                    command.EmpresaId = command.EmpresaIdSolicitante.Value;
                }

                // So admin concede admin: senao um operador se promoveria mandando
                // "perfil": "admin" no JSON, sem passar pela tela.
                if (Entidades.Usuario.EhPerfilAdmin(command.Perfil) && !command.SolicitanteEhAdmin)
                {
                    response.AddErro("Apenas um administrador pode criar outro administrador.");
                    return response;
                }

                _unitOfWork.BeginTransaction();
                var validator = new CriaUsuarioValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Email precisa ser unico: o login busca por email com QueryFirstOrDefault, entao
                // dois usuarios com o mesmo email tornam o login nao-deterministico (e trocar a
                // senha de um nao afeta o outro).
                if (!string.IsNullOrWhiteSpace(command.Email))
                {
                    var jaExiste = await _unitOfWork.Usuario.ObterPorEmail(command.Email);
                    if (jaExiste != null)
                    {
                        response.AddErro("Já existe um usuário cadastrado com esse e-mail.");
                        _unitOfWork.Rollback();
                        return response;
                    }
                }

                // Aplica BCrypt na senha antes de salvar
                if (!string.IsNullOrEmpty(command.SenhaHash))
                {
                    command.SenhaHash = BCrypt.Net.BCrypt.HashPassword(command.SenhaHash);
                }

                await _unitOfWork.Usuario.Incluir(new Entidades.Usuario(command));

                response.AddValue(new CriaUsuarioResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro ao criar usuário: {ex.Message}");
            }

            return response;
        }
    }
}


