using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Recebe os eventos de pagamento da Cakto. É por aqui que cliente novo entra na plataforma:
    // pagou, a conta nasce.
    //
    // Sem [ApiController] pelo mesmo motivo do webhook da Meta: com ele, um payload fora do
    // formato esperado vira 400 automático antes do método rodar, e a Cakto passa a reenviar o
    // evento 5 vezes achando que estamos fora do ar.
    [Route("api/webhook/cakto")]
    [AllowAnonymous]
    // O rate limit global mediria por IP e a Cakto manda tudo do mesmo lugar: um pico legítimo
    // de vendas viraria 429, e evento de pagamento perdido é dinheiro perdido.
    [DisableRateLimiting]
    public class CaktoWebhookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CaktoWebhookController> _logger;

        public CaktoWebhookController(IMediator mediator, IConfiguration configuration, ILogger<CaktoWebhookController> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receber()
        {
            string corpo;
            using (var leitor = new StreamReader(Request.Body, Encoding.UTF8))
            {
                corpo = await leitor.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(corpo))
            {
                _logger.LogWarning("Cakto: webhook chamado com corpo vazio");
                return BadRequest();
            }

            ProcessaEventoCaktoCommand? command;
            try
            {
                command = JsonConvert.DeserializeObject<ProcessaEventoCaktoCommand>(corpo);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Cakto: payload fora do formato esperado");
                return BadRequest();
            }

            if (command == null) return BadRequest();

            var segredoEsperado = _configuration["CaktoConfiguration:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(segredoEsperado))
            {
                // Mesma lição do AppSecret da Meta: sem a configuração, responder 200 faria a
                // Cakto marcar o evento como entregue e a venda sumiria em silêncio. 500 mantém
                // o evento na fila de reenvio dela até alguém configurar.
                _logger.LogError("Cakto: CaktoConfiguration:WebhookSecret nao configurado — evento nao processado");
                return StatusCode(500);
            }

            if (!SegredoConfere(command.Secret, segredoEsperado))
            {
                // Não é a Cakto (ou o segredo mudou): 401 sem detalhe.
                _logger.LogWarning("Cakto: webhook recebido com segredo invalido");
                return Unauthorized();
            }

            // Guarda o corpo para auditoria SEM o segredo -- ele não pode acabar no banco.
            command.PayloadOriginal = RemoverSegredo(corpo);
            command.Secret = null;

            var resultado = await _mediator.Send(command);

            if (resultado.HasValidations)
            {
                var mensagens = string.Join(" | ", resultado.Erros.Select(e => e.Mensagem));
                _logger.LogError("Cakto: falha ao processar evento {Evento}: {Erros}", command.Evento, mensagens);

                // 500 pede reenvio; 400 seria "não tente de novo", e aí a venda se perderia.
                return StatusCode(500);
            }

            return Ok(new { recebido = true, acao = resultado.Value?.Acao });
        }

        // Comparação em tempo fixo: comparar com == vaza, pelo tempo de resposta, o quanto do
        // segredo o atacante acertou.
        private static bool SegredoConfere(string? recebido, string esperado)
        {
            if (string.IsNullOrEmpty(recebido)) return false;

            var bytesRecebido = Encoding.UTF8.GetBytes(recebido);
            var bytesEsperado = Encoding.UTF8.GetBytes(esperado);

            return CryptographicOperations.FixedTimeEquals(bytesRecebido, bytesEsperado);
        }

        private static string RemoverSegredo(string corpo)
        {
            try
            {
                var objeto = Newtonsoft.Json.Linq.JObject.Parse(corpo);
                objeto.Remove("secret");
                return objeto.ToString(Formatting.None);
            }
            catch
            {
                // Se não deu para reescrever o JSON, é mais seguro não guardar nada.
                return string.Empty;
            }
        }
    }
}
