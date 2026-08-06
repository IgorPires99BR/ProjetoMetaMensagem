using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta;

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

        [HttpPost("enviar-midia-meta")]
        public async Task<IActionResult> EnviarMidia(
            [FromForm] IFormFile arquivo,
            [FromForm] string celular,
            [FromForm] Guid empresaId,
            [FromForm] Guid contatoId,
            [FromForm] string tipoMidia)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { erro = "Arquivo não informado." });
                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await arquivo.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }

                var command = new EnviarMidiaMetaCommand
                {
                    Celular = celular,
                    EmpresaId = empresaId,
                    ContatoId = contatoId,
                    Arquivo = bytes,
                    MimeType = string.IsNullOrEmpty(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
                    TipoMidia = tipoMidia
                };

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
    }
}
