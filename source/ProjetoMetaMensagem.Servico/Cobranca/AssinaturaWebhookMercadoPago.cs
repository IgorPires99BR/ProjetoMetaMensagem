using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Validacao do cabecalho x-signature ("ts=...,v1=...") do webhook do Mercado Pago:
    // v1 = HMAC-SHA256 hex, com a assinatura secreta, de "id:{data.id};request-id:{x-request-id};ts:{ts};".
    // O data.id vem da query string; se for alfanumerico (orders: "ORD01..."), vai em minusculas.
    // Valor ausente sai do manifesto junto com o rotulo, como manda a documentacao.
    public static class AssinaturaWebhookMercadoPago
    {
        public static bool Valida(string? xSignature, string? xRequestId, string? dataId, string segredo)
        {
            if (string.IsNullOrWhiteSpace(xSignature) || string.IsNullOrWhiteSpace(segredo)) return false;

            string? ts = null, v1 = null;
            foreach (var parte in xSignature.Split(','))
            {
                var chaveValor = parte.Split('=', 2);
                if (chaveValor.Length != 2) continue;
                var chave = chaveValor[0].Trim();
                if (chave == "ts") ts = chaveValor[1].Trim();
                else if (chave == "v1") v1 = chaveValor[1].Trim();
            }
            if (string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(v1)) return false;

            var id = dataId?.Trim();
            if (!string.IsNullOrEmpty(id) && id.All(char.IsLetterOrDigit)) id = id.ToLowerInvariant();

            var manifesto = new StringBuilder();
            if (!string.IsNullOrEmpty(id)) manifesto.Append($"id:{id};");
            if (!string.IsNullOrWhiteSpace(xRequestId)) manifesto.Append($"request-id:{xRequestId.Trim()};");
            manifesto.Append($"ts:{ts};");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segredo));
            var esperado = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifesto.ToString()))).ToLowerInvariant();

            // Tempo fixo: == vazaria pelo tempo de resposta quanto do hash o atacante acertou.
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(esperado), Encoding.UTF8.GetBytes(v1.ToLowerInvariant()));
        }
    }
}
