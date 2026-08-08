using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Leads
{
    [ApiController]
    [Route("api/leads")]
    public class LeadsController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<LeadsController> _logger;

        public LeadsController(IMediator mediator, ILogger<LeadsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        [HttpGet("{companyId}")]
        public async Task<IActionResult> Listar(string companyId)
        {
            try
            {
                ListaConversaPorIdCommand command = new ListaConversaPorIdCommand();

                command.CompanyId = companyId;

                var resultado = await _mediator.Send(command);

                if (resultado != null)
                    return this.ValidateResponse((int)HttpStatusCode.Created, resultado);

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "LeadsController.Listar"), tipo = "Servico" });
            }
        }
    }
}
