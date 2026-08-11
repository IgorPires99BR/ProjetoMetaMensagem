using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta;
using ProjetoMetaMensagem.WebAPI.Common;

namespace ProjetoMetaMensagem.Controllers.DisparadorDeMensagem
{
    [ApiController]
    [Route("api/disparador")]
    public class DisparadorController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<DisparadorController> _logger;

        public DisparadorController(IMediator mediator, ILogger<DisparadorController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("enviar-mensagem-meta")]
        public async Task<IActionResult> Enviar([FromBody] EnviarMensagemMetaCommand command)
        {
            try
            {
                command.UsuarioIdSolicitante = this.UsuarioIdDoEscopo();

                var resultado = await _mediator.Send(command);

                if (resultado != null && resultado.Erros.Count == 0)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "DisparadorController.Enviar"), tipo = "Servico" });
            }
        }

        [HttpPost("CriaTemplate")]
        public async Task<IActionResult> CriaTemplate([FromBody] CriarTemplateMetaCommand command)
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "DisparadorController.CriaTemplate"), tipo = "Servico" });
            }
        }

        [HttpPost("EnviarMensagemTemplate")]
        public async Task<IActionResult> EnviarMensagemTemplate([FromBody] EnviarMensagemTemplateMetaCommand command)
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "DisparadorController.EnviarMensagemTemplate"), tipo = "Servico" });
            }
        }

        [HttpPost("EnviarMensagemTemplateLote")]
        public async Task<IActionResult> EnviarMensagemTemplateLote([FromBody] EnviarMensagemTemplateMetaLoteCommand command)
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "DisparadorController.EnviarMensagemTemplateLote"), tipo = "Servico" });
            }
        }

        [HttpPost("enviar-midia-meta")]
        public async Task<IActionResult> EnviarMidia(
            [FromForm] IFormFile? arquivo,
            [FromForm] string? celular,
            [FromForm] Guid? empresaId,
            [FromForm] Guid? contatoId,
            [FromForm] string? tipoMidia)
        {
            try
            {
                // Os parametros sao nullable de proposito: com tipos nao-anulaveis (Guid,
                // string), se o multipart chegasse com algum campo vazio o [ApiController]
                // interceptava ANTES desse metodo rodar e devolvia um ProblemDetails generico
                // do ASP.NET (formato diferente do erro customizado do projeto), escondendo
                // qual campo realmente faltou.
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { mensagem = "Arquivo não informado.", tipo = "Negocio" });
                }
                if (string.IsNullOrWhiteSpace(celular) || empresaId == null || empresaId == Guid.Empty
                    || contatoId == null || contatoId == Guid.Empty || string.IsNullOrWhiteSpace(tipoMidia))
                {
                    return BadRequest(new { mensagem = "Celular, empresaId, contatoId e tipoMidia são obrigatórios.", tipo = "Negocio" });
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
                    EmpresaId = empresaId.Value,
                    ContatoId = contatoId.Value,
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "DisparadorController.EnviarMidia"), tipo = "Servico" });
            }
        }
    }
}
