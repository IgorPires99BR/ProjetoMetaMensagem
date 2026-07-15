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

        public WebhookConfigsController(IMediator mediator) => _mediator = mediator;

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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpDelete("api/v2/webhook/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaWebhookCommand { Id = id });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
