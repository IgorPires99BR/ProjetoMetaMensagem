using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

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
        public async Task<IActionResult> Listar(Guid IdEmpresa, [FromQuery] Guid? numeroId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaFlowsCommand(IdEmpresa) { NumeroId = numeroId });

                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "FlowsController.Listar"), tipo = "Servico" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Incluir(CriaFlowCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "FlowsController.Incluir"), tipo = "Servico" });
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

                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "FlowsController.Alterar"), tipo = "Servico" });
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

                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "FlowsController.Excluir"), tipo = "Servico" });
            }
        }
    }
}
