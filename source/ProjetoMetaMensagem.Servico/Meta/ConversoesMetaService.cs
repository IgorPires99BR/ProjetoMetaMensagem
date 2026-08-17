using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta
{
    // Manda o evento de compra para a Meta pelo servidor, em vez de depender do navegador.
    //
    // Dois motivos para ser server-side: o checkout roda no domínio da Cakto, onde nosso pixel não
    // entra; e venda que nasce de conversa no WhatsApp não passa por navegador nenhum -- para essas,
    // a Meta exige `action_source: business_messaging` com o ctwa_clid do clique no anúncio.
    public class ConversoesMetaService : IConversoesMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly MetaConversoesConfiguration _config;
        private readonly ILogger<ConversoesMetaService> _logger;

        public ConversoesMetaService(HttpClient httpClient, IOptions<MetaConversoesConfiguration> options, ILogger<ConversoesMetaService> logger)
        {
            _httpClient = httpClient;
            _config = options.Value;
            _logger = logger;
        }

        public async Task<bool> ReportarCompraAsync(
            string emailComprador,
            string? telefoneComprador,
            decimal valor,
            string? idPedido,
            string? ctwaClid,
            string? fbc,
            string? fbp)
        {
            if (string.IsNullOrWhiteSpace(_config.PixelId) || string.IsNullOrWhiteSpace(_config.AccessToken))
            {
                _logger.LogWarning("Conversions API sem PixelId/AccessToken configurados — venda de {Email} nao reportada a Meta", emailComprador);
                return false;
            }

            // A Meta exige e-mail e telefone com hash SHA-256; o ctwa_clid e os cookies vão em claro.
            var userData = new Dictionary<string, object?>
            {
                ["em"] = new[] { Hash(emailComprador) },
            };

            if (!string.IsNullOrWhiteSpace(telefoneComprador))
                userData["ph"] = new[] { Hash(TelefoneHelper.FormatarParaMeta(telefoneComprador!)) };

            // ctwa_clid identifica o clique no anúncio que trouxe a conversa. Quando ele existe, a
            // origem da ação é a conversa (business_messaging), não o site -- mandar como "website"
            // faria a Meta descartar a atribuição.
            var veioDeConversa = !string.IsNullOrWhiteSpace(ctwaClid);

            if (veioDeConversa) userData["ctwa_clid"] = ctwaClid;
            if (!string.IsNullOrWhiteSpace(fbc)) userData["fbc"] = fbc;
            if (!string.IsNullOrWhiteSpace(fbp)) userData["fbp"] = fbp;

            var evento = new Dictionary<string, object?>
            {
                ["event_name"] = "Purchase",
                ["event_time"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["action_source"] = veioDeConversa ? "business_messaging" : "website",
                ["user_data"] = userData,
                ["custom_data"] = new Dictionary<string, object?>
                {
                    ["value"] = valor,
                    ["currency"] = "BRL",
                },
            };

            // event_id e o que a Meta usa pra desduplicar: se o mesmo pedido for reportado duas
            // vezes (reenvio de webhook, por exemplo), conta uma venda so.
            if (!string.IsNullOrWhiteSpace(idPedido)) evento["event_id"] = idPedido;
            if (veioDeConversa) evento["messaging_channel"] = "whatsapp";

            var payload = new Dictionary<string, object?>
            {
                ["data"] = new[] { evento },
                ["access_token"] = _config.AccessToken,
            };

            if (!string.IsNullOrWhiteSpace(_config.TestEventCode))
                payload["test_event_code"] = _config.TestEventCode;

            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var resposta = await _httpClient.PostAsync(
                    $"{_config.PixelId}/events",
                    new StringContent(json, Encoding.UTF8, "application/json"));

                var corpo = await resposta.Content.ReadAsStringAsync();

                if (!resposta.IsSuccessStatusCode)
                {
                    _logger.LogError("Conversions API recusou a venda de {Email}: {Corpo}", emailComprador, corpo);
                    return false;
                }

                _logger.LogInformation(
                    "Venda de {Email} reportada a Meta ({Origem})", emailComprador,
                    veioDeConversa ? "conversa do WhatsApp" : "site");

                return true;
            }
            catch (Exception ex)
            {
                // Falha aqui não pode afetar a venda: ela já aconteceu e a conta já está liberada.
                _logger.LogError(ex, "Falha ao reportar a venda de {Email} a Meta", emailComprador);
                return false;
            }
        }

        private static string Hash(string valor)
        {
            var normalizado = (valor ?? string.Empty).Trim().ToLowerInvariant();
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizado));

            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
