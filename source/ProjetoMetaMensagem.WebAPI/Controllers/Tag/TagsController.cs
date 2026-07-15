using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Tag
{
    [ApiController]
    [Route("api/tag")]
    public class TagsController : Controller
    {
        private readonly IMediator _mediator;
        public TagsController(IMediator mediator) => _mediator = mediator;

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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpDelete("excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaTagCommand { Id = id });
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("associar-contato")]
        public async Task<IActionResult> AssociarContato([FromBody] AssociarTagsContatoCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
