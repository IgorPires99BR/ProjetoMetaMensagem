using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.Login;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Auth.Login
{
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/auth/login")]
        public async Task<IActionResult> Enviar([FromBody] LoginCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);

            return this.ValidateResponse((int)HttpStatusCode.BadRequest, null);
        }
    }
}
