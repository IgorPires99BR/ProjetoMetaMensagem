using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Webhook
{
    [ApiController]
    public class WebhookConfigsController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<WebhookConfigsController> _logger;

        public WebhookConfigsController(IMediator mediator, ILogger<WebhookConfigsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/v2/webhook/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaWebhookCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "WebhookConfigsController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpGet("api/v2/webhook/obter/{empresaId}")]
        public async Task<IActionResult> Obter(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaWebhookCommand { EmpresaId = empresaId });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "WebhookConfigsController.Obter"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/v2/webhook/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                var resultado = await _mediator.Send(new DeletaWebhookCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "WebhookConfigsController.Excluir"), tipo = "Servico" });
            }
        }
    }
}
