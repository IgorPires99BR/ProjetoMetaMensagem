using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Campanha
{
    [ApiController]
    public class CampanhasController : Controller
    {
        private readonly IMediator _mediator;

        public CampanhasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/campanha/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaCampanhaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpGet("api/campanha/listar/{empresaId}")]
        public async Task<IActionResult> Listar([FromRoute] Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaCampanhaCommand { EmpresaId = empresaId });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPut("api/campanha/cancelar/{id}")]
        public async Task<IActionResult> Cancelar([FromRoute] Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new CancelaCampanhaCommand { Id = id });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
