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
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsappWebhookController> _logger;

        public WhatsappWebhookController(IMediator mediator, IHubContext<ChatHub> hubContext, IConfiguration configuration, ILogger<WhatsappWebhookController> logger)
        {
            _mediator = mediator;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult VerificarWebhook(
    [FromQuery(Name = "hub.mode")] string? mode = null,
    [FromQuery(Name = "hub.verify_token")] string? verifyToken = null,
    [FromQuery(Name = "hub.challenge")] string? challenge = null)
        {
            Console.WriteLine($"[Webhook] Mode: {mode} | Token Recebido: {verifyToken} | Challenge: {challenge}");

            string? tokenConfigurado = _configuration["ApiWhatsappConnectionConfiguration:VerifyToken"];

            if (mode == "subscribe" && verifyToken == tokenConfigurado)
            {
                // Retorna o challenge com HTTP 200 OK
                return Ok(challenge);
            }

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
                if (resultado != null && resultado.Erros.Count() == 0 && resultado.Value != null)
                {
                    // Broadcast por empresa: cada front so entra no grupo da sua propria empresa
                    // (ver ChatHub.EntrarNoGrupo), entao aqui notificamos so quem precisa saber.
                    foreach (var mensagem in resultado.Value.MensagensSalvas)
                    {
                        await _hubContext.Clients.Group(mensagem.EmpresaId.ToString())
                            .SendAsync("ReceberNovaMensagem", new
                            {
                                contatoId = mensagem.ContatoId,
                                telefone = mensagem.TelefoneRemetente,
                                conteudo = mensagem.Conteudo,
                                dataRecebimento = mensagem.DataRecebimento
                            });
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Retorna 200 propositalmente: a Meta reenvia (e pode desativar o webhook)
                // se receber erro. O detalhe fica registrado no log do servidor.
                _logger.LogError(ex, "Falha ao processar webhook da Meta. Payload: {@Payload}", payload);
                return Ok();
            }
        }
    }
}