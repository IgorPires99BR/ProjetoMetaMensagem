using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook;
using ProjetoMetaMensagem.WebAPI.Hubs;

namespace ProjetoMetaMensagem.Controllers
{
    [ApiController]
    [Route("api/webhook/whatsapp")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<ChatHub> _hubContext;

        public WhatsappWebhookController(IMediator mediator, IHubContext<ChatHub> hubContext)
        {
            _mediator = mediator;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> ReceberMensagem([FromBody] RecebeMensagemWebhookCommand payload)
        {
            // Validação rápida de segurança para evitar processar requisições vazias
            if (payload == null)
            {
                return Ok();
            }

            try
            {
                // Despacha para o Handler processar as regras de negócio e persistência
                var resultado = await _mediator.Send(payload);

                await _hubContext.Clients.Group(resultado.Value.Mensagem.ToString())
                            .SendAsync("ReceberNovaMensagem", resultado.Value.Mensagem);

                // Retorno crítico exigido pela Meta: Avisa que a requisição foi processada/recebida
                return Ok();
            }
            catch (Exception ex)
            {
                // TODO: Logue o erro aqui com seu serviço de log (ex: _logger.LogError) 
                // para você monitorar se o banco falhou ou algo do tipo.

                // IMPORTANTE: Sempre retorne Ok() para a Meta, prevenindo retries infinitos 
                // e a suspensão temporária do seu Webhook no painel deles.
                return Ok();
            }
        }
    }
}