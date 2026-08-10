using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate;
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Incluir"), tipo = "Servico" });
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
                    return BadRequest(new { mensagem = "Nenhum arquivo enviado.", tipo = "Negocio" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.UploadMidiaExemplo"), tipo = "Servico" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Listar"), tipo = "Servico" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpPut("api/template/{id}")]
        public async Task<IActionResult> Editar(Guid id, [FromBody] AtualizaTemplateCommand command)
        {
            try
            {
                command.TemplateId = id;
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Editar"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/template/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaTemplateCommand
                {
                    TemplateId = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.Excluir"), tipo = "Servico" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.ListarConexoes"), tipo = "Servico" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.IncluirConexao"), tipo = "Servico" });
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
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "TemplatesController.ExcluirConexao"), tipo = "Servico" });
            }
        }
    }
}
