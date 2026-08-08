using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ExcluiTemplateConexao;
using ProjetoMetaMensagem.Dominio.UseCases.Template.UploadMidiaTemplate;
using ProjetoMetaMensagem.WebAPI.Common;
using System.IO;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Template
{
    [ApiController]
    public class TemplatesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(IMediator mediator, ILogger<TemplatesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/template/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaTemplateCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Incluir") });
            }
        }

        [HttpPost("api/template/upload-midia-exemplo")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadMidiaExemplo([FromForm] Guid empresaId, [FromForm] IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { erro = "Nenhum arquivo enviado." });
                }

                using var memoryStream = new MemoryStream();
                await arquivo.CopyToAsync(memoryStream);

                var command = new UploadMidiaTemplateCommand
                {
                    EmpresaId = empresaId,
                    Arquivo = memoryStream.ToArray(),
                    MimeType = arquivo.ContentType
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.UploadMidiaExemplo") });
            }
        }

        [HttpGet("api/template/Listar/{empresaId}")]
        public async Task<IActionResult> Listar(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaTemplateCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Listar") });
            }
        }

        [HttpPut("api/template/AtualizaTemplateMeta/{empresaId}")]
        public async Task<IActionResult> Alterar(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new AtualizaTemplateMetaCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Alterar") });
            }
        }

        [HttpGet("api/template/conexoes/{empresaId}")]
        public async Task<IActionResult> ListarConexoes(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaTemplateConexoesCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.ListarConexoes") });
            }
        }

        [HttpPost("api/template/conexoes")]
        public async Task<IActionResult> IncluirConexao([FromBody] CriaTemplateConexaoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.IncluirConexao") });
            }
        }

        [HttpDelete("api/template/conexoes/{id}")]
        public async Task<IActionResult> ExcluirConexao(Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                var resultado = await _mediator.Send(new ExcluiTemplateConexaoCommand(id)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TemplatesController.ExcluirConexao") });
            }
        }
    }
}
