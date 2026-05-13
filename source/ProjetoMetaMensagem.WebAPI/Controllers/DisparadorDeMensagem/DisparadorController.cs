using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;

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

        [HttpPost("CriaTemplate")]
        public async Task<IActionResult> CriaTemplate([FromBody] CriarTemplateMetaCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpPost("EnviarMensagemTemplate")]
        public async Task<IActionResult> EnviarMensagemTemplate([FromBody] EnviarMensagemTemplateMetaCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

    }
}
