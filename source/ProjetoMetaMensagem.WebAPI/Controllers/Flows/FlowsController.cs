using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Flows
{
    [ApiController]
    [Route("api/config/flow")]
    public class FlowsController : Controller
    {
        private readonly IMediator _mediator;

        public FlowsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{IdEmpresa}")]
        public async Task<IActionResult> Listar(Guid IdEmpresa)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaFlowsCommand(IdEmpresa));

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Incluir(CriaFlowCommand command)
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

        [HttpPut]
        public async Task<IActionResult> Alterar(AlteraFlowCommand command)
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

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaFlowCommand(id));

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
