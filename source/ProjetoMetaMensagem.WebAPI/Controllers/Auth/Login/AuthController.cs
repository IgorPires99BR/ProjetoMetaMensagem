using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.Login;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Auth.Login
{
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    // Sem lockout/contagem de tentativas no login (BCrypt.Verify direto) -- este limite por IP
    // e a unica barreira contra tentativa exaustiva de senha.
    [EnableRateLimiting("auth")]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        public async Task<IActionResult> Enviar([FromBody] LoginCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return this.ValidateResponse((int)HttpStatusCode.Created, resultado);

                return this.ValidateResponse((int)HttpStatusCode.BadRequest, null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AuthController.Enviar"), tipo = "Servico" });
            }
        }
    }
}
