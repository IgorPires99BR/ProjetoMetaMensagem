using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Auth.EsqueceuASenha
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("/api/auth/forgot-password")]
        public async Task<IActionResult> Enviar([FromBody] EsqueceuASenhaCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }
    }
}
