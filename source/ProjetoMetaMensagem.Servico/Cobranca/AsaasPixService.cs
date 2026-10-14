using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Singleton: guarda um HttpClient e o cache de customers. Um HttpClient novo por chamada
    // esgotaria sockets num disparo em lote.
    public class AsaasPixService : IPixService
    {
        private readonly AsaasConfiguration _config;
        private readonly ILogger<AsaasPixService> _logger;
        private readonly HttpClient _cliente;

        // CPF/CNPJ -> id do customer no Asaas. Sem isso cada disparo faria uma busca a mais por
        // contato; o customer nao muda de id, entao o cache nunca fica errado.
        private readonly ConcurrentDictionary<string, string> _customers = new();

        // walletId -> cliente padrao da empresa. O documento da subconta nao muda.
        private readonly ConcurrentDictionary<string, PagadorPix> _pagadoresPadrao = new();

        public AsaasPixService(IOptions<AsaasConfiguration> options, ILogger<AsaasPixService> logger)
        {
            _config = options.Value;
            _logger = logger;

            // Recicla conexoes: senao uma troca de DNS do Asaas nunca seria vista.
            _cliente = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(10) })
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _cliente.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _config.UserAgent);
        }

        public string LinkPagamento(string txid) => $"{BasePublica}/pix/{txid}";
        public string UrlQrCode(string txid) => $"{BasePublica}/pix/{txid}/qrcode.png";

        private string BasePublica => _config.UrlPublicaApi.TrimEnd('/');

        public async Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca)
        {
            // Split fixo maior que o liquido e bloqueado pelo Asaas DEPOIS do pagamento -- o
            // cliente pagaria e o dinheiro ficaria preso. Barra antes de gerar o Pix.
            var comissao = cobranca.Valor - cobranca.ValorRepasse;
            if (comissao < _config.TaxaPix)
                throw new PixException($"a comissão da plataforma (R$ {comissao.ToString("N2", Brasil)}) não cobre a taxa do Asaas por Pix (R$ {_config.TaxaPix.ToString("N2", Brasil)}). Ajuste a comissão da empresa.");

            var customer = await ObterOuCriarCustomerAsync(cobranca.Pagador);

            var corpo = new JObject
            {
                ["customer"] = customer,
                ["billingType"] = "PIX",
                ["value"] = Math.Round(cobranca.Valor, 2),
                ["dueDate"] = cobranca.Vencimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["externalReference"] = cobranca.Txid,
                // Encargos ja estao no valor (CalculoCobrancaPix). Sem zerar, valeriam a multa e
                // os juros configurados na conta da plataforma, e o valor sairia diferente da mensagem.
                ["fine"] = new JObject { ["value"] = 0 },
                ["interest"] = new JObject { ["value"] = 0 },
                ["split"] = new JArray
                {
                    new JObject
                    {
                        ["walletId"] = cobranca.WalletIdRepasse,
                        ["fixedValue"] = Math.Round(cobranca.ValorRepasse, 2),
                        ["externalReference"] = cobranca.Txid
                    }
                }
            };
            if (!string.IsNullOrWhiteSpace(cobranca.Descricao))
                corpo["description"] = cobranca.Descricao.Length > 500 ? cobranca.Descricao[..500] : cobranca.Descricao;

            var pagamento = await EnviarAsync(HttpMethod.Post, "/payments", corpo, "gerar a cobrança");
            var id = pagamento["id"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
                throw new PixException("O Asaas criou a cobrança mas não devolveu o identificador dela.");

            try
            {
                var qr = await EnviarAsync(HttpMethod.Get, $"/payments/{Uri.EscapeDataString(id)}/pixQrCode", null, "gerar o QR Code Pix");
                var copiaECola = qr["payload"]?.ToString();
                if (string.IsNullOrWhiteSpace(copiaECola))
                    throw new PixException("O Asaas gerou a cobrança mas não devolveu o código Pix copia e cola.");

                return new PixGerado(id, copiaECola, pagamento["status"]?.ToString() ?? "PENDING");
            }
            catch
            {
                // Cobranca sem QR nunca vai ser enviada: nao pode ficar aberta no Asaas.
                try { await CancelarCobrancaAsync(id); }
                catch (Exception ex) { _logger.LogWarning(ex, "AsaasPix: cobranca {IdExterno} ficou aberta sem QR", id); }
                throw;
            }
        }

        public async Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno)
        {
            var resposta = await EnviarAsync(HttpMethod.Get, $"/payments/{Uri.EscapeDataString(idExterno)}", null, "consultar a cobrança");

            var status = resposta["status"]?.ToString() ?? string.Empty;

            decimal? valorPago = null;
            DateTime? pagoEm = null;
            if (status == "RECEIVED")
            {
                if (decimal.TryParse(resposta["value"]?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                    valorPago = v;

                // O Asaas so devolve a DATA do pagamento. Pago hoje = agora (a baixa roda minutos
                // depois); dia anterior fica a meia-noite daquele dia.
                var data = resposta["clientPaymentDate"]?.ToString() ?? resposta["paymentDate"]?.ToString();
                if (DateTime.TryParseExact(data, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dia))
                {
                    var agora = HorarioBrasilia.Agora();
                    pagoEm = dia.Date == agora.Date ? agora : dia.Date;
                }
            }

            return new ConsultaCobrancaPix(status, valorPago, pagoEm, null);
        }

        public async Task CancelarCobrancaAsync(string idExterno)
        {
            await EnviarAsync(HttpMethod.Delete, $"/payments/{Uri.EscapeDataString(idExterno)}", null, "cancelar a cobrança");
        }

        public async Task<SubcontaCriada> CriarSubcontaAsync(NovaSubconta subconta)
        {
            var corpo = new JObject
            {
                ["name"] = subconta.Nome,
                ["email"] = subconta.Email,
                ["cpfCnpj"] = subconta.CpfCnpj,
                ["mobilePhone"] = subconta.Celular,
                ["incomeValue"] = Math.Round(subconta.FaturamentoMensal, 2),
                ["address"] = subconta.Endereco,
                ["addressNumber"] = subconta.Numero,
                ["province"] = subconta.Bairro,
                ["postalCode"] = subconta.Cep
            };
            if (subconta.DataNascimento.HasValue)
                corpo["birthDate"] = subconta.DataNascimento.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(subconta.TipoEmpresa))
                corpo["companyType"] = subconta.TipoEmpresa;
            if (!string.IsNullOrWhiteSpace(subconta.Complemento))
                corpo["complement"] = subconta.Complemento;

            var resposta = await EnviarAsync(HttpMethod.Post, "/accounts", corpo, "criar a subconta");

            var id = resposta["id"]?.ToString();
            var walletId = resposta["walletId"]?.ToString();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(walletId))
                throw new PixException("O Asaas criou a subconta mas não devolveu o walletId.");

            // A documentacao mostra a chave ora na raiz, ora dentro de accessToken.
            var apiKey = resposta["apiKey"]?.ToString() ?? resposta["accessToken"]?["apiKey"]?.ToString() ?? string.Empty;
            if (apiKey == string.Empty)
                _logger.LogWarning("AsaasPix: subconta {ContaId} criada sem apiKey na resposta", id);

            return new SubcontaCriada(id, walletId, apiKey);
        }

        public async Task<PagadorPix> PagadorPadraoDaSubcontaAsync(string walletId)
        {
            if (_pagadoresPadrao.TryGetValue(walletId, out var existente)) return existente;

            var busca = await EnviarAsync(HttpMethod.Get, $"/accounts?walletId={Uri.EscapeDataString(walletId)}&limit=1", null, "consultar a subconta da empresa");
            var conta = (busca["data"] as JArray)?.FirstOrDefault();
            var cpfCnpj = conta?["cpfCnpj"]?.ToString();
            if (string.IsNullOrWhiteSpace(cpfCnpj))
                throw new PixException("não foi possível obter o CNPJ da subconta Asaas da empresa para usar como cliente padrão.");

            var pagador = new PagadorPix($"Clientes {conta!["name"]?.ToString() ?? "da empresa"}".Trim(), cpfCnpj, null);
            _pagadoresPadrao[walletId] = pagador;
            return pagador;
        }

        public async Task TestarCredenciaisAsync()
        {
            await EnviarAsync(HttpMethod.Get, "/finance/balance", null, "conferir a conta");
        }

        private async Task<string> ObterOuCriarCustomerAsync(PagadorPix pagador)
        {
            if (_customers.TryGetValue(pagador.CpfCnpj, out var existente)) return existente;

            var busca = await EnviarAsync(HttpMethod.Get, $"/customers?cpfCnpj={Uri.EscapeDataString(pagador.CpfCnpj)}&limit=1", null, "buscar o cliente");
            var id = (busca["data"] as JArray)?.FirstOrDefault()?["id"]?.ToString();

            if (string.IsNullOrWhiteSpace(id))
            {
                var corpo = new JObject
                {
                    ["name"] = pagador.Nome,
                    ["cpfCnpj"] = pagador.CpfCnpj,
                    // Quem avisa o cliente e a mensagem do WhatsApp; sem isso o Asaas mandaria
                    // e-mail/SMS proprios da cobranca, em nome da plataforma.
                    ["notificationDisabled"] = true
                };
                if (!string.IsNullOrWhiteSpace(pagador.Email))
                    corpo["email"] = pagador.Email;

                var criado = await EnviarAsync(HttpMethod.Post, "/customers", corpo, "cadastrar o cliente");
                id = criado["id"]?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                    throw new PixException("O Asaas cadastrou o cliente mas não devolveu o identificador dele.");
            }

            _customers[pagador.CpfCnpj] = id;
            return id;
        }

        private async Task<JObject> EnviarAsync(HttpMethod metodo, string caminho, JObject? corpo, string operacao)
        {
            if (!_config.Configurada)
                throw new PixException("A integração com o Asaas não está configurada no servidor (Asaas:ApiKey). Avise o suporte.");

            using var requisicao = new HttpRequestMessage(metodo, UrlApi + caminho);
            requisicao.Headers.TryAddWithoutValidation("access_token", _config.ApiKey);
            if (corpo != null)
                requisicao.Content = new StringContent(corpo.ToString(Formatting.None), Encoding.UTF8, "application/json");

            HttpResponseMessage resposta;
            try
            {
                resposta = await _cliente.SendAsync(requisicao);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogWarning(ex, "AsaasPix: falha de rede ao {Operacao}", operacao);
                throw new PixException($"Não foi possível falar com o Asaas para {operacao}. Tente novamente em instantes.", null, ex);
            }

            using (resposta)
            {
                var conteudo = await resposta.Content.ReadAsStringAsync();

                if ((int)resposta.StatusCode == 401)
                {
                    _logger.LogWarning("AsaasPix: HTTP 401 ao {Operacao}", operacao);
                    throw new PixException("O Asaas recusou a chave de API da plataforma. Avise o suporte.", 401);
                }

                if (!resposta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AsaasPix: HTTP {Status} ao {Operacao} ({Caminho}): {Corpo}",
                        (int)resposta.StatusCode, operacao, caminho, conteudo);
                    throw new PixException($"O Asaas recusou {operacao}: {MensagemDoAsaas(conteudo, (int)resposta.StatusCode)}", (int)resposta.StatusCode);
                }

                if (string.IsNullOrWhiteSpace(conteudo)) return new JObject();
                try
                {
                    // Sem conversao automatica de data: o Newtonsoft transformaria "2026-10-06"
                    // em DateTime e o ToString() voltaria no formato da cultura do servidor (ja
                    // gravou pagamento de 06/10 como 09/07 na integracao do Itau).
                    using var leitor = new JsonTextReader(new System.IO.StringReader(conteudo)) { DateParseHandling = DateParseHandling.None };
                    return JObject.Load(leitor);
                }
                catch (JsonException)
                {
                    throw new PixException($"Resposta inesperada do Asaas ao {operacao}.", (int)resposta.StatusCode);
                }
            }
        }

        private static readonly CultureInfo Brasil = new("pt-BR");

        private bool Producao => string.Equals(_config.Ambiente, AsaasConfiguration.AmbienteProducao, StringComparison.OrdinalIgnoreCase);
        private string UrlApi => (Producao ? _config.UrlApiProducao : _config.UrlApiSandbox).TrimEnd('/');

        // Erro do Asaas: { "errors": [ { "code": "...", "description": "..." } ] }.
        private static string MensagemDoAsaas(string conteudo, int status)
        {
            try
            {
                var json = JObject.Parse(conteudo);
                var descricoes = (json["errors"] as JArray)?
                    .Select(e => e["description"]?.ToString())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();
                if (descricoes is { Count: > 0 }) return string.Join(" ", descricoes);
            }
            catch (JsonException)
            {
            }

            return $"HTTP {status}";
        }
    }
}
