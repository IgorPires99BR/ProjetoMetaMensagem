using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Dashboard
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : Controller
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator) => _mediator = mediator;

        [HttpGet("metricas/{empresaId}")]
        public async Task<IActionResult> ObterMetricas(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ObterMetricasCommand(empresaId));
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
