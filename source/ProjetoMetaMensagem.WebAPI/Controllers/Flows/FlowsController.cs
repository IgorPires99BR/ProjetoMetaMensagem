using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
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
            var resultado = await _mediator.Send(new ListaFlowsCommand(IdEmpresa));

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Incluir(CriaFlowCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpPut]
        public async Task<IActionResult> Alterar(AlteraFlowCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }
    }
}
