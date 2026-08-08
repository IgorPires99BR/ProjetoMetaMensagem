using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Response<LoginResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(IUnitOfWork unitOfWork, ITokenService tokenService, ILogger<LoginHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Response<LoginResult>> Handle(LoginCommand request)
        {
            var response = new Response<LoginResult>();

            try
            {
                var usuario = await _unitOfWork.Usuario.ObterPorEmail(request.email);

                if (usuario == null || string.IsNullOrEmpty(usuario.SenhaHash))
                {
                    response.AddErro("Usuário e senha não encontrados no banco de dados.");
                    return response;
                }

                // Sem essa verificacao, qualquer senha era aceita para um email valido —
                // o handler nunca comparava a senha enviada com o hash salvo.
                var senhaValida = BCrypt.Net.BCrypt.Verify(request.password, usuario.SenhaHash);
                if (!senhaValida)
                {
                    response.AddErro("Usuário e senha não encontrados no banco de dados.");
                    return response;
                }

                var token = _tokenService.GerarToken(usuario.Id.ToString(), usuario.Email, usuario.Nome, usuario.EmpresaId.ToString(), usuario.IsAdmin?.ToString() ?? "false");

                response.AddValue(new LoginResult(usuario, token));
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(LoginHandler)));
            }

            return response;
        }
    }
}
