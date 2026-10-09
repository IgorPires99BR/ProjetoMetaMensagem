using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Servicos;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Webhook da API Pix do Itau (um por empresa, cadastrado ao salvar os dados bancarios).
    // O Itau chama a URL cadastrada + "/pix" (padrao Bacen), por isso as duas rotas.
    //
    // Sem autenticacao propria de proposito: o Itau exige mTLS e o Render nao repassa o
    // certificado do cliente, entao nao ha como provar que foi o Itau. Em vez disso o corpo e
    // tratado so como "aviso": cada txid e reconsultado no Itau antes da baixa (BaixaCobrancaPix).
    //
    // Sem [ApiController] pelo mesmo motivo do webhook da Cakto: payload fora do formato nao
    // pode virar 400 automatico e fazer o Itau reenviar em loop.
    [AllowAnonymous]
    [DisableRateLimiting]
    public class ItauPixWebhookController : ControllerBase
    {
        private readonly IBaixaCobrancaPix _baixa;
        private readonly ILogger<ItauPixWebhookController> _logger;

        public ItauPixWebhookController(IBaixaCobrancaPix baixa, ILogger<ItauPixWebhookController> logger)
        {
            _baixa = baixa;
            _logger = logger;
        }

        [HttpPost("api/webhook/itau-pix/{empresaId}/pix")]
        [HttpPost("api/webhook/itau-pix/{empresaId}")]
        public async Task<IActionResult> Receber([FromRoute] Guid empresaId)
        {
            string corpo;
            using (var leitor = new StreamReader(Request.Body, Encoding.UTF8))
            {
                corpo = await leitor.ReadToEndAsync();
            }

            List<string> txids;
            try
            {
                txids = (JObject.Parse(corpo)["pix"] as JArray ?? new JArray())
                    .Select(p => p["txid"]?.ToString())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t!)
                    .Take(100)
                    .ToList();
            }
            catch (JsonException)
            {
                _logger.LogWarning("ItauPix: webhook da empresa {EmpresaId} com corpo fora do formato", empresaId);
                return Ok();
            }

            // Pix recebido na chave sem txid (transferencia avulsa) nao e cobranca nossa.
            if (txids.Count == 0) return Ok();

            try
            {
                await _baixa.ProcessarWebhookAsync(empresaId, txids);
            }
            catch (Exception ex)
            {
                // 200 mesmo assim: a consulta periodica pega o que ficou para tras, e um 500 so
                // faria o Itau insistir no mesmo aviso.
                _logger.LogError(ex, "ItauPix: falha ao processar webhook da empresa {EmpresaId}", empresaId);
            }

            return Ok();
        }
    }
}
