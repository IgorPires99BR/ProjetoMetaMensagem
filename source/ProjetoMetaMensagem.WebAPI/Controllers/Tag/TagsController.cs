using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag;
using ProjetoMetaMensagem.WebAPI.Common;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Tag
{
    [ApiController]
    [Route("api/tag")]
    public class TagsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TagsController> _logger;

        public TagsController(IMediator mediator, ILogger<TagsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaTagCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return StatusCode(201, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TagsController.Incluir") });
            }
        }

        [HttpGet("listar/{empresaId}")]
        public async Task<IActionResult> Listar(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaTagCommand(empresaId));
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TagsController.Listar") });
            }
        }

        [HttpDelete("excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                var resultado = await _mediator.Send(new DeletaTagCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TagsController.Excluir") });
            }
        }

        [HttpPost("associar-contato")]
        public async Task<IActionResult> AssociarContato([FromBody] AssociarTagsContatoCommand cmd)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                cmd.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(cmd);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "TagsController.AssociarContato") });
            }
        }
    }
}
