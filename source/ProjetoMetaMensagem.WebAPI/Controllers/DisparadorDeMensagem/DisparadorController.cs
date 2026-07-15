using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;

namespace ProjetoMetaMensagem.Controllers.DisparadorDeMensagem
{
    [ApiController]
    [Route("api/disparador")]
    public class DisparadorController : Controller
    {
        private readonly IMediator _mediator;

        public DisparadorController(IMediator mediator)
        {
            _mediator = mediator; 
        }

        [HttpPost("enviar-mensagem-meta")]
        public async Task<IActionResult> Enviar([FromBody] EnviarMensagemMetaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null && resultado.Erros.Count == 0)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("CriaTemplate")]
        public async Task<IActionResult> CriaTemplate([FromBody] CriarTemplateMetaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("EnviarMensagemTemplate")]
        public async Task<IActionResult> EnviarMensagemTemplate([FromBody] EnviarMensagemTemplateMetaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("EnviarMensagemTemplateLote")]
        public async Task<IActionResult> EnviarMensagemTemplateLote([FromBody] EnviarMensagemTemplateMetaLoteCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

    }
}
