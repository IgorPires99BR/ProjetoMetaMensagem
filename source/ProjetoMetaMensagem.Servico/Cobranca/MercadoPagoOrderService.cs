using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Pix pela API de Orders do Mercado Pago (POST /v1/orders, type online, processing_mode
    // automatic). Singleton pelo mesmo motivo do AsaasPixService: um HttpClient so.
    public class MercadoPagoOrderService : IProvedorCobranca
    {
        // Limites do Mercado Pago para a validade do Pix da order.
        private static readonly TimeSpan ValidadeMinima = TimeSpan.FromMinutes(31);
        private static readonly TimeSpan ValidadeMaxima = TimeSpan.FromDays(30);

        private readonly MercadoPagoConfiguration _config;
        private readonly ILogger<MercadoPagoOrderService> _logger;
        private readonly HttpClient _cliente;

        public MercadoPagoOrderService(IOptions<MercadoPagoConfiguration> options, ILogger<MercadoPagoOrderService> logger)
        {
            _config = options.Value;
            _logger = logger;

            _cliente = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(10) })
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public string Nome => ProvedorCobranca.MercadoPago;
        public bool SuportaSplit => false;

        public async Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca)
        {
            // Repasse pedido aqui seria silenciosamente ignorado e a empresa nao receberia.
            if (!string.IsNullOrWhiteSpace(cobranca.WalletIdRepasse))
                throw new PixException("o Mercado Pago não faz repasse automático para a subconta da empresa.");

            var email = EmailValido(cobranca.Pagador.Email) ? cobranca.Pagador.Email!.Trim()
                      : EmailValido(_config.EmailPagadorPadrao) ? _config.EmailPagadorPadrao!.Trim()
                      : throw new PixException("o contato não tem e-mail e o e-mail padrão de pagador do Mercado Pago não está configurado. Avise o suporte.");

            var valor = Valor(cobranca.Valor);
            var pagador = new JObject { ["email"] = email };
            var documento = DocumentoHelper.SoDigitos(cobranca.Pagador.CpfCnpj);
            if (DocumentoHelper.CpfCnpjValido(documento))
                pagador["identification"] = new JObject { ["type"] = documento!.Length == 11 ? "CPF" : "CNPJ", ["number"] = documento };

            var corpo = new JObject
            {
                ["type"] = "online",
                ["processing_mode"] = "automatic",
                ["external_reference"] = cobranca.Txid,
                ["total_amount"] = valor,
                ["payer"] = pagador,
                ["transactions"] = new JObject
                {
                    ["payments"] = new JArray
                    {
                        new JObject
                        {
                            ["amount"] = valor,
                            ["payment_method"] = new JObject { ["id"] = "pix", ["type"] = "bank_transfer" },
                            ["expiration_time"] = Validade(cobranca.Vencimento, HorarioBrasilia.Agora())
                        }
                    }
                }
            };

            // Idempotencia pelo txid: um retry depois de timeout devolve a mesma order em vez de
            // abrir um segundo Pix para a mesma cobranca.
            var order = await EnviarAsync(HttpMethod.Post, "/v1/orders", corpo, "gerar a cobrança", cobranca.Txid);
            var id = order["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
                throw new PixException("O Mercado Pago criou a order mas não devolveu o identificador dela.");

            var metodo = PrimeiroPagamento(order)?["payment_method"];
            var copiaECola = metodo?["qr_code"]?.ToString();
            if (string.IsNullOrWhiteSpace(copiaECola))
            {
                // Order sem Pix nunca vai ser enviada: nao pode ficar aberta.
                try { await CancelarCobrancaAsync(id); }
                catch (Exception ex) { _logger.LogWarning(ex, "MercadoPago: order {IdExterno} ficou aberta sem Pix", id); }
                throw new PixException($"O Mercado Pago não gerou o código Pix (situação da order: {order["status"]}/{order["status_detail"]}).");
            }

            return new PixGerado(id, copiaECola, order["status"]?.ToString() ?? string.Empty, metodo?["ticket_url"]?.ToString());
        }

        public async Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno)
        {
            var order = await EnviarAsync(HttpMethod.Get, $"/v1/orders/{Uri.EscapeDataString(idExterno)}", null, "consultar a cobrança");

            var status = order["status"]?.ToString() ?? string.Empty;
            var detalhe = order["status_detail"]?.ToString() ?? string.Empty;
            var pagamento = PrimeiroPagamento(order);

            // processed/accredited = dinheiro creditado na conta. Pago parcialmente ou em analise
            // nao da baixa: a consulta periodica volta nele.
            var paga = status == "processed" && detalhe == "accredited";

            decimal? valorPago = null;
            DateTime? pagoEm = null;
            if (paga)
            {
                valorPago = Decimal(order["total_paid_amount"]) ?? Decimal(pagamento?["paid_amount"]) ?? Decimal(order["total_amount"]);
                pagoEm = Data(pagamento?["date_approved"]) ?? Data(order["last_updated_date"]);
            }

            return new ConsultaCobrancaPix($"{status}/{detalhe}", valorPago, pagoEm, null,
                PagaNoProvedor: paga, AguardandoPagamento: status == "action_required");
        }

        public async Task CancelarCobrancaAsync(string idExterno)
        {
            await EnviarAsync(HttpMethod.Post, $"/v1/orders/{Uri.EscapeDataString(idExterno)}/cancel", null, "cancelar a cobrança", $"cancelar-{idExterno}");
        }

        public async Task TestarCredenciaisAsync()
        {
            await EnviarAsync(HttpMethod.Get, "/users/me", null, "conferir a conta");
        }

        // Duracao ISO 8601 ate 23:59:59 do vencimento (horario de Brasilia), dentro dos limites
        // do Mercado Pago. Em horas e minutos: e o formato dos exemplos da documentacao.
        public static string Validade(DateTime vencimento, DateTime agora)
        {
            var duracao = vencimento.Date.AddDays(1).AddSeconds(-1) - agora;
            if (duracao < ValidadeMinima) duracao = ValidadeMinima;
            if (duracao > ValidadeMaxima) duracao = ValidadeMaxima;

            var horas = (int)duracao.TotalHours;
            var minutos = duracao.Minutes;
            return minutos == 0 ? $"PT{horas}H" : $"PT{horas}H{minutos}M";
        }

        private static JToken? PrimeiroPagamento(JObject order) => (order["transactions"]?["payments"] as JArray)?.FirstOrDefault();

        private static string Valor(decimal valor) => Math.Round(valor, 2).ToString("0.00", CultureInfo.InvariantCulture);

        private static decimal? Decimal(JToken? token) =>
            decimal.TryParse(token?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v > 0m ? v : null;

        private static DateTime? Data(JToken? token) =>
            DateTimeOffset.TryParse(token?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var momento)
                ? HorarioBrasilia.ParaBrasilia(momento)
                : null;

        private static bool EmailValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { return new MailAddress(email.Trim()).Address == email.Trim(); }
            catch (FormatException) { return false; }
        }

        private async Task<JObject> EnviarAsync(HttpMethod metodo, string caminho, JObject? corpo, string operacao, string? chaveIdempotencia = null)
        {
            if (!_config.Configurada)
                throw new PixException("A integração com o Mercado Pago não está configurada no servidor (MercadoPago:AccessToken). Avise o suporte.");

            using var requisicao = new HttpRequestMessage(metodo, _config.UrlApi.TrimEnd('/') + caminho);
            requisicao.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_config.AccessToken!.Trim()}");
            if (chaveIdempotencia != null)
                requisicao.Headers.TryAddWithoutValidation("X-Idempotency-Key", chaveIdempotencia);
            if (corpo != null)
                requisicao.Content = new StringContent(corpo.ToString(Formatting.None), Encoding.UTF8, "application/json");

            HttpResponseMessage resposta;
            try
            {
                resposta = await _cliente.SendAsync(requisicao);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogWarning(ex, "MercadoPago: falha de rede ao {Operacao}", operacao);
                throw new PixException($"Não foi possível falar com o Mercado Pago para {operacao}. Tente novamente em instantes.", null, ex);
            }

            using (resposta)
            {
                var conteudo = await resposta.Content.ReadAsStringAsync();

                if ((int)resposta.StatusCode == 401)
                {
                    _logger.LogWarning("MercadoPago: HTTP 401 ao {Operacao}", operacao);
                    throw new PixException("O Mercado Pago recusou o access token da plataforma. Avise o suporte.", 401);
                }

                if (!resposta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("MercadoPago: HTTP {Status} ao {Operacao} ({Caminho}): {Corpo}",
                        (int)resposta.StatusCode, operacao, caminho, conteudo);
                    throw new PixException($"O Mercado Pago recusou {operacao}: {MensagemDoMercadoPago(conteudo, (int)resposta.StatusCode)}", (int)resposta.StatusCode);
                }

                if (string.IsNullOrWhiteSpace(conteudo)) return new JObject();
                try
                {
                    // Sem conversao automatica de data (mesmo motivo do AsaasPixService).
                    using var leitor = new JsonTextReader(new System.IO.StringReader(conteudo)) { DateParseHandling = DateParseHandling.None };
                    return JObject.Load(leitor);
                }
                catch (JsonException)
                {
                    throw new PixException($"Resposta inesperada do Mercado Pago ao {operacao}.", (int)resposta.StatusCode);
                }
            }
        }

        // Orders: { "errors": [ { "code", "message", "details": [...] } ] }; APIs mais antigas:
        // { "message": "...", "error": "..." }.
        private static string MensagemDoMercadoPago(string conteudo, int status)
        {
            try
            {
                var json = JObject.Parse(conteudo);
                var mensagens = (json["errors"] as JArray)?
                    .Select(e =>
                    {
                        var detalhes = (e["details"] as JArray)?.Select(d => d.ToString()).Where(d => d != string.Empty).ToList();
                        var mensagem = e["message"]?.ToString();
                        return detalhes is { Count: > 0 } ? $"{mensagem} ({string.Join("; ", detalhes)})" : mensagem;
                    })
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToList();
                if (mensagens is { Count: > 0 }) return string.Join(" ", mensagens);

                var mensagemSimples = json["message"]?.ToString();
                if (!string.IsNullOrWhiteSpace(mensagemSimples)) return mensagemSimples;
            }
            catch (JsonException)
            {
            }

            return $"HTTP {status}";
        }
    }
}
