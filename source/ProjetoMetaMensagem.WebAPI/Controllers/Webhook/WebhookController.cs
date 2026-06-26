using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Webhook
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : Controller
    {
        private readonly IFlowOrchestratorService _flowOrchestrator;
        private readonly IUnitOfWork _unitOfWork;

        public WebhookController(IFlowOrchestratorService flowOrchestrator, IUnitOfWork unitOfWork)
        {
            _flowOrchestrator = flowOrchestrator;
            _unitOfWork = unitOfWork;
        }

        // GET api/webhook?hub.mode=subscribe&hub.challenge=123&hub.verify_token=TOKEN
        // Usado pela Meta para verificar o webhook
        [HttpGet]
        public IActionResult VerificarWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            try
            {
                if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(challenge) || string.IsNullOrEmpty(verifyToken))
                    return BadRequest("Parametros invalidos.");

                if (mode == "subscribe")
                {
                    // Nota: Em producao, valide o verifyToken contra o TokenWebhookLocal da empresa
                    // Cada empresa tem um TokenWebhookLocal (GUID unico)
                    // Aqui podemos buscar a empresa pelo token e retornar o challenge se valido

                    // Por enquanto, aceita qualquer token nao vazio
                    return Content(challenge, "text/plain");
                }

                return BadRequest("Modo invalido.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        // POST api/webhook
        // Recebe notificacoes da Meta (mensagens recebidas, status de entrega, etc.)
        [HttpPost]
        public async Task<IActionResult> ReceberNotificacao([FromBody] object payload)
        {
            try
            {
                // A implementacao completa do webhook depende do payload exato da Meta Cloud API
                // Estrutura esperada:
                // {
                //   "entry": [{
                //     "changes": [{
                //       "value": {
                //         "messages": [{
                //           "from": "5511999998888",
                //           "text": { "body": "Ola" },
                //           "type": "text"
                //         }],
                //         "metadata": {
                //           "phone_number_id": "123456789"
                //         }
                //       }
                //     }]
                //   }]
                // }

                // Log para visualizacao do payload recebido (uteis para debug)
                Console.WriteLine($"Webhook recebido: {payload}");

                // Por se tratar de um payload complexo e que varia entre versoes da API,
                // a desserializacao detalhada sera feita apos confirmacao do formato exato.
                // Retornamos 200 OK para que a Meta nao reenvie a notificacao.

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
