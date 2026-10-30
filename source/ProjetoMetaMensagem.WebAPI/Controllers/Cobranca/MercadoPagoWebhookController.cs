using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Servico.Cobranca;
using ProjetoMetaMensagem.Servico.Configuration;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Webhook da conta Mercado Pago da plataforma, um so para todas as empresas: o
    // external_reference da order e o txid, que diz de quem ela e. Cadastrado no painel (Suas
    // integracoes > Webhooks) com o evento "Order (Mercado Pago)"; a assinatura secreta de la vai
    // em MercadoPago:WebhookSecret. Mesmo com assinatura valida o corpo e so "aviso" -- a order e
    // reconsultada antes da baixa (BaixaCobrancaPix).
    //
    // Sem [ApiController] pelo mesmo motivo do webhook do Asaas: payload fora do formato nao pode
    // virar 400 automatico, e resposta diferente de 200/201 faz o Mercado Pago reenviar.
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly IBaixaCobrancaPix _baixa;
        private readonly MercadoPagoConfiguration _config;
        private readonly ILogger<MercadoPagoWebhookController> _logger;

        public MercadoPagoWebhookController(IBaixaCobrancaPix baixa, IOptions<MercadoPagoConfiguration> config, ILogger<MercadoPagoWebhookController> logger)
        {
            _baixa = baixa;
            _config = config.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("api/webhook/mercadopago")]
        public async Task<IActionResult> Receber()
        {
            string corpo;
            using (var leitor = new StreamReader(Request.Body, Encoding.UTF8))
            {
                corpo = await leitor.ReadToEndAsync();
            }

            JObject evento;
            try
            {
                evento = JObject.Parse(corpo);
            }
            catch (JsonException)
            {
                _logger.LogWarning("MercadoPago: webhook com corpo fora do formato");
                return Ok();
            }

            // A assinatura usa o data.id da query string; o corpo so cobre a falta dele.
            var dataId = Request.Query["data.id"].ToString();
            if (string.IsNullOrWhiteSpace(dataId)) dataId = evento["data"]?["id"]?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_config.WebhookSecret)
                && !AssinaturaWebhookMercadoPago.Valida(Request.Headers["x-signature"].ToString(), Request.Headers["x-request-id"].ToString(), dataId, _config.WebhookSecret))
            {
                _logger.LogWarning("MercadoPago: webhook com assinatura invalida descartado (data.id {DataId})", dataId);
                return Unauthorized();
            }

            // So orders interessam; outros topicos (pagamentos avulsos, testes do painel) so confirmam.
            var tipo = evento["type"]?.ToString() ?? Request.Query["type"].ToString();
            if (!string.Equals(tipo, "order", StringComparison.OrdinalIgnoreCase)) return Ok();

            // Order sem external_reference foi criada fora desta API. Qualquer acao (processed,
            // canceled, expired...) passa pela baixa: ela reconsulta e so marca o que estiver pago.
            var txid = evento["data"]?["external_reference"]?.ToString();
            if (string.IsNullOrWhiteSpace(txid)) return Ok();

            try
            {
                await _baixa.ProcessarWebhookAsync(new[] { txid });
            }
            catch (Exception ex)
            {
                // 200 mesmo assim: a consulta periodica pega o que ficou para tras.
                _logger.LogError(ex, "MercadoPago: falha ao processar webhook da order {DataId} (txid {Txid})", dataId, txid);
            }

            return Ok();
        }
    }
}
