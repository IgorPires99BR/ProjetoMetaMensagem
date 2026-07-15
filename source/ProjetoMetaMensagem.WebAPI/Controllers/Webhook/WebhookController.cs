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
        private readonly IConfiguration _configuration;

        public WhatsappWebhookController(IMediator mediator, IHubContext<ChatHub> hubContext, IConfiguration configuration)
        {
            _mediator = mediator;
            _hubContext = hubContext;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult VerificarWebhook(
        [FromQuery(Name = "hub.mode")] string? mode = null,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken = null,
        [FromQuery(Name = "hub.challenge")] string? challenge = null)
        {
            // Log para monitorar o que está chegando no Render
            Console.WriteLine($"[Webhook] Mode: {mode} | Token Recebido: {verifyToken} | Challenge: {challenge}");

            // O "Token" configurado no seu appsettings.json
            string? tokenConfigurado = _configuration["ApiWhatsappConnectionConfiguration:VerifyToken"];

            if (mode == "subscribe" && verifyToken == tokenConfigurado)
            {
                // Retorna APENAS o código do challenge como texto puro com status 200 (OK)
                return Content(challenge ?? string.Empty, "text/plain");
            }

            // Se as condições não forem atendidas, retorna 403 Forbidden
            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> ReceberMensagem([FromBody] RecebeMensagemWebhookCommand payload)
        {
            if (payload == null) return Ok();

            try
            {
                var resultado = await _mediator.Send(payload);

                // Verifica se a operação foi bem sucedida e se retornou dados válidos
                if (resultado != null && resultado.Erros.Count() == 0)
                {
                    // Sugestão: O grupo do SignalR geralmente deve ser o "empresaId" ou o ID do chat, 
                    // usar o conteúdo da mensagem como nome do grupo pode não notificar a tela certa.
                    await _hubContext.Clients.Group("GrupoNotificacao Chat ou Empresa")
                                .SendAsync("ReceberNovaMensagem", resultado.Value.Mensagem);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Seu log aqui
                return Ok();
            }
        }
    }
}