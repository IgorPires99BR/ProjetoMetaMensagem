using ProjetoMetaMensagem.Dominio.Exemplo;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.EnviarMensagemMeta;
using Microsoft.AspNetCore.Mvc;

namespace ProjetoMetaMensagem.Controllers.DisparadorDeMensagem
{
    [ApiController]
    [Route("Disparador")]
    public class DisparadorController : Controller
    {
        private readonly IMediator _mediator;

        public DisparadorController(IMediator mediator)
        {
            _mediator = mediator; 
        }

        [HttpPost("enviar-meta")]
        public async Task<IActionResult> Enviar([FromBody] EnviarMensagemMetaCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

    }
}
