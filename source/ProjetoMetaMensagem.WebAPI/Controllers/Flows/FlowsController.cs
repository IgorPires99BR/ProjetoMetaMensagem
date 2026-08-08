using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using ProjetoMetaMensagem.WebAPI.Common;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Flows
{
    [ApiController]
    [Route("api/config/flow")]
    public class FlowsController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<FlowsController> _logger;

        public FlowsController(IMediator mediator, ILogger<FlowsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
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
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "FlowsController.Listar") });
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
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "FlowsController.Incluir") });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Alterar(AlteraFlowCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "FlowsController.Alterar") });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaFlowCommand(id)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });

                if (resultado != null)
                    return Ok(resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "FlowsController.Excluir") });
            }
        }
    }
}
