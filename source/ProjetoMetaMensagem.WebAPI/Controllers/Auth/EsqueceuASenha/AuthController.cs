using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Auth.EsqueceuASenha
{
    [ApiController]
    [Route("[controller]")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("/api/auth/forgot-password")]
        public async Task<IActionResult> Enviar([FromBody] EsqueceuASenhaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return this.ValidateResponse((int)HttpStatusCode.Created, resultado);

                return this.ValidateResponse((int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AuthController.Enviar"), tipo = "Servico" });
            }
        }
    }
}
