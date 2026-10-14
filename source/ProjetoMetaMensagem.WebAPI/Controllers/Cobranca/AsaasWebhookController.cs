using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Webhook da conta Asaas da plataforma: um so para todas as empresas (o externalReference
    // da cobranca e o txid, que diz de quem ela e). Cadastrado no painel do Asaas (Integracoes >
    // Webhooks) com o evento PAYMENT_RECEIVED e o token de Asaas:WebhookToken. Mesmo com o
    // token certo, o corpo e so "aviso" -- cada cobranca e reconsultada antes da baixa
    // (BaixaCobrancaPix).
    //
    // Sem [ApiController] pelo mesmo motivo do webhook da Cakto: payload fora do formato nao
    // pode virar 400 automatico. Resposta diferente de 200 faz o Asaas reenviar e, depois de 15
    // falhas seguidas, PAUSA a fila inteira de webhooks da conta.
    public class AsaasWebhookController : ControllerBase
    {
        private readonly IBaixaCobrancaPix _baixa;
        private readonly AsaasConfiguration _config;
        private readonly ILogger<AsaasWebhookController> _logger;

        public AsaasWebhookController(IBaixaCobrancaPix baixa, IOptions<AsaasConfiguration> config, ILogger<AsaasWebhookController> logger)
        {
            _baixa = baixa;
            _config = config.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("api/webhook/asaas")]
        public async Task<IActionResult> Receber()
        {
            if (!string.IsNullOrWhiteSpace(_config.WebhookToken)
                && !TokenConfere(Request.Headers["asaas-access-token"].ToString(), _config.WebhookToken))
            {
                _logger.LogWarning("AsaasPix: webhook com token invalido descartado");
                return Unauthorized();
            }

            string corpo;
            using (var leitor = new StreamReader(Request.Body, Encoding.UTF8))
            {
                corpo = await leitor.ReadToEndAsync();
            }

            string? txid;
            try
            {
                var evento = JObject.Parse(corpo);
                // So o pagamento recebido interessa; outros eventos (criada, vencida...) so confirmam.
                if (evento["event"]?.ToString() != "PAYMENT_RECEIVED") return Ok();
                txid = evento["payment"]?["externalReference"]?.ToString();
            }
            catch (JsonException)
            {
                _logger.LogWarning("AsaasPix: webhook com corpo fora do formato");
                return Ok();
            }

            // Cobranca sem externalReference foi criada fora desta API (painel do Asaas).
            if (string.IsNullOrWhiteSpace(txid)) return Ok();

            try
            {
                await _baixa.ProcessarWebhookAsync(new[] { txid });
            }
            catch (Exception ex)
            {
                // 200 mesmo assim: a consulta periodica pega o que ficou para tras, e erro aqui
                // contaria para a pausa da fila do Asaas.
                _logger.LogError(ex, "AsaasPix: falha ao processar webhook do txid {Txid}", txid);
            }

            return Ok();
        }

        // Comparacao em tempo fixo: == vaza pelo tempo de resposta o quanto do token o atacante acertou.
        private static bool TokenConfere(string recebido, string esperado)
        {
            if (string.IsNullOrEmpty(recebido)) return false;
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(recebido), Encoding.UTF8.GetBytes(esperado));
        }
    }
}
