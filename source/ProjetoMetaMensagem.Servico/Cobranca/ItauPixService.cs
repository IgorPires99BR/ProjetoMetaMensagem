using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Singleton: guarda um HttpClient (com o certificado mTLS) e o token OAuth por empresa.
    // Um HttpClient novo por chamada abriria um handshake TLS com certificado a cada Pix e
    // esgotaria sockets num disparo em lote.
    public class ItauPixService : IItauPixService
    {
        private readonly ItauPixConfiguration _config;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<ItauPixService> _logger;

        private readonly ConcurrentDictionary<string, HttpClient> _clientes = new();
        private readonly ConcurrentDictionary<string, (string Token, DateTime ExpiraEmUtc)> _tokens = new();
        private readonly SemaphoreSlim _travaToken = new(1, 1);

        public ItauPixService(IOptions<ItauPixConfiguration> options, ICriptografiaService criptografia, ILogger<ItauPixService> logger)
        {
            _config = options.Value;
            _criptografia = criptografia;
            _logger = logger;
        }

        public string LinkPagamento(string txid) => $"{BasePublica}/pix/{txid}";
        public string UrlQrCode(string txid) => $"{BasePublica}/pix/{txid}/qrcode.png";
        // O Itau acrescenta "/pix" ao final da URL cadastrada (padrao Bacen).
        public string UrlWebhook(Guid empresaId) => $"{BasePublica}/api/webhook/itau-pix/{empresaId}";

        private string BasePublica => _config.UrlPublicaApi.TrimEnd('/');

        public async Task<PixGerado> CriarCobrancaAsync(DadosBancariosEmpresa dados, string txid, decimal valor, int expiracaoSegundos, string? solicitacaoPagador)
        {
            var corpo = new JObject
            {
                ["calendario"] = new JObject { ["expiracao"] = expiracaoSegundos },
                ["valor"] = new JObject { ["original"] = valor.ToString("0.00", CultureInfo.InvariantCulture) },
                ["chave"] = dados.ChavePix
            };
            if (!string.IsNullOrWhiteSpace(solicitacaoPagador))
                corpo["solicitacaoPagador"] = solicitacaoPagador.Length > 140 ? solicitacaoPagador[..140] : solicitacaoPagador;

            var resposta = await EnviarAsync(dados, HttpMethod.Put, $"/cob/{txid}", corpo, "gerar o Pix");

            var copiaECola = resposta["pixCopiaECola"]?.ToString();
            if (string.IsNullOrWhiteSpace(copiaECola))
                throw new ItauPixException("O Itaú gerou a cobrança mas não devolveu o código Pix copia e cola.");

            return new PixGerado(resposta["txid"]?.ToString() ?? txid, copiaECola, resposta["status"]?.ToString() ?? "ATIVA");
        }

        public async Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(DadosBancariosEmpresa dados, string txid)
        {
            var resposta = await EnviarAsync(dados, HttpMethod.Get, $"/cob/{txid}", null, "consultar o Pix");

            var status = resposta["status"]?.ToString() ?? string.Empty;

            // Uma cob imediata so aceita um pagamento; se vier mais de um (devolucao e novo
            // pagamento, por exemplo), soma os valores e fica com o horario do ultimo.
            decimal? valorPago = null;
            DateTime? pagoEm = null;
            string? endToEndId = null;
            if (resposta["pix"] is JArray pagamentos)
            {
                foreach (var pagamento in pagamentos)
                {
                    if (decimal.TryParse(pagamento["valor"]?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                        valorPago = (valorPago ?? 0m) + v;

                    if (DateTimeOffset.TryParse(pagamento["horario"]?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var horario))
                        pagoEm = HorarioBrasilia.ParaBrasilia(horario);

                    endToEndId = pagamento["endToEndId"]?.ToString() ?? endToEndId;
                }
            }

            return new ConsultaCobrancaPix(status, valorPago, pagoEm, endToEndId);
        }

        public async Task CancelarCobrancaAsync(DadosBancariosEmpresa dados, string txid)
        {
            var corpo = new JObject { ["status"] = "REMOVIDA_PELO_USUARIO_RECEBEDOR" };
            await EnviarAsync(dados, HttpMethod.Patch, $"/cob/{txid}", corpo, "cancelar o Pix");
        }

        public async Task ConfigurarWebhookAsync(DadosBancariosEmpresa dados, string urlWebhook)
        {
            var corpo = new JObject { ["webhookUrl"] = urlWebhook };
            await EnviarAsync(dados, HttpMethod.Put, $"/webhook/{Uri.EscapeDataString(dados.ChavePix ?? string.Empty)}", corpo, "cadastrar o webhook");
        }

        public async Task TestarCredenciaisAsync(DadosBancariosEmpresa dados)
        {
            _tokens.TryRemove(ChaveDoCliente(dados), out _);
            await ObterTokenAsync(dados, ObterCliente(dados));
        }

        private async Task<JObject> EnviarAsync(DadosBancariosEmpresa dados, HttpMethod metodo, string caminho, JObject? corpo, string operacao)
        {
            var cliente = ObterCliente(dados);

            // Uma nova tentativa com token novo se o Itau recusar o atual (revogado ou expirado
            // antes do previsto) -- sem isso a cobranca falharia ate o cache vencer sozinho.
            for (var tentativa = 1; ; tentativa++)
            {
                var token = await ObterTokenAsync(dados, cliente);

                using var requisicao = new HttpRequestMessage(metodo, UrlApi(dados) + caminho);
                requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requisicao.Headers.Add("x-itau-apikey", dados.ClientId);
                requisicao.Headers.Add("x-correlationID", Guid.NewGuid().ToString());
                if (corpo != null)
                    requisicao.Content = new StringContent(corpo.ToString(Formatting.None), Encoding.UTF8, "application/json");

                HttpResponseMessage resposta;
                try
                {
                    resposta = await cliente.SendAsync(requisicao);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    _logger.LogWarning(ex, "ItauPix: falha de rede ao {Operacao} (empresa {EmpresaId})", operacao, dados.EmpresaId);
                    throw new ItauPixException($"Não foi possível falar com o Itaú para {operacao}. Tente novamente em instantes.", null, ex);
                }

                using (resposta)
                {
                    var conteudo = await resposta.Content.ReadAsStringAsync();

                    if ((int)resposta.StatusCode == 401 && tentativa == 1)
                    {
                        _tokens.TryRemove(ChaveDoCliente(dados), out _);
                        continue;
                    }

                    if (!resposta.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("ItauPix: HTTP {Status} ao {Operacao} (empresa {EmpresaId}): {Corpo}",
                            (int)resposta.StatusCode, operacao, dados.EmpresaId, conteudo);
                        throw new ItauPixException($"O Itaú recusou {operacao}: {MensagemDoItau(conteudo, (int)resposta.StatusCode)}", (int)resposta.StatusCode);
                    }

                    if (string.IsNullOrWhiteSpace(conteudo)) return new JObject();
                    try
                    {
                        // Sem conversao automatica de data: o Newtonsoft transformaria o "horario"
                        // ISO em DateTime e o ToString() voltaria no formato da cultura do
                        // servidor -- o pagamento de 06/10 chegou a ser gravado como 09/07.
                        using var leitor = new JsonTextReader(new System.IO.StringReader(conteudo)) { DateParseHandling = DateParseHandling.None };
                        return JObject.Load(leitor);
                    }
                    catch (JsonException)
                    {
                        throw new ItauPixException($"Resposta inesperada do Itaú ao {operacao}.", (int)resposta.StatusCode);
                    }
                }
            }
        }

        private async Task<string> ObterTokenAsync(DadosBancariosEmpresa dados, HttpClient cliente)
        {
            var chave = ChaveDoCliente(dados);
            if (_tokens.TryGetValue(chave, out var atual) && atual.ExpiraEmUtc > DateTime.UtcNow)
                return atual.Token;

            await _travaToken.WaitAsync();
            try
            {
                // Outro disparo do mesmo lote pode ter renovado enquanto este esperava a trava.
                if (_tokens.TryGetValue(chave, out atual) && atual.ExpiraEmUtc > DateTime.UtcNow)
                    return atual.Token;

                if (string.IsNullOrWhiteSpace(dados.ClientId) || string.IsNullOrWhiteSpace(dados.ClientSecretCriptografado))
                    throw new ItauPixException("Client ID ou Client Secret do Itaú não cadastrados nos dados bancários da empresa.");

                var secret = _criptografia.Descriptografar(dados.ClientSecretCriptografado);

                using var requisicao = new HttpRequestMessage(HttpMethod.Post, UrlToken(dados))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = dados.ClientId,
                        ["client_secret"] = secret
                    })
                };
                requisicao.Headers.Add("x-correlationID", Guid.NewGuid().ToString());

                HttpResponseMessage resposta;
                try
                {
                    resposta = await cliente.SendAsync(requisicao);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    _logger.LogWarning(ex, "ItauPix: falha de rede ao autenticar (empresa {EmpresaId})", dados.EmpresaId);
                    throw new ItauPixException("Não foi possível conectar ao Itaú. Confira o certificado cadastrado e tente novamente.", null, ex);
                }

                using (resposta)
                {
                    var conteudo = await resposta.Content.ReadAsStringAsync();
                    if (!resposta.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("ItauPix: HTTP {Status} ao autenticar (empresa {EmpresaId}): {Corpo}",
                            (int)resposta.StatusCode, dados.EmpresaId, conteudo);
                        throw new ItauPixException("O Itaú recusou as credenciais (Client ID, Client Secret ou certificado). Confira os dados bancários da empresa.", (int)resposta.StatusCode);
                    }

                    var json = JObject.Parse(conteudo);
                    var token = json["access_token"]?.ToString();
                    if (string.IsNullOrWhiteSpace(token))
                        throw new ItauPixException("O Itaú não devolveu o token de acesso.");

                    var expiraEm = json["expires_in"]?.Value<int?>() ?? 300;
                    // Margem de 1 minuto: token que vence no meio de um lote derrubaria os ultimos envios.
                    _tokens[chave] = (token, DateTime.UtcNow.AddSeconds(Math.Max(30, expiraEm - 60)));
                    return token;
                }
            }
            finally
            {
                _travaToken.Release();
            }
        }

        private HttpClient ObterCliente(DadosBancariosEmpresa dados)
        {
            return _clientes.GetOrAdd(ChaveDoCliente(dados), _ =>
            {
                var handler = new SocketsHttpHandler
                {
                    // Recicla conexoes: senao uma troca de certificado/DNS do Itau nunca seria vista.
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10)
                };

                var certificado = CarregarCertificado(dados);
                if (certificado != null)
                {
                    handler.SslOptions.ClientCertificates = new X509CertificateCollection { certificado };
                }

                return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            });
        }

        private X509Certificate2? CarregarCertificado(DadosBancariosEmpresa dados)
        {
            if (string.IsNullOrWhiteSpace(dados.CertificadoPem) || string.IsNullOrWhiteSpace(dados.ChavePrivadaCriptografada))
            {
                // Producao sempre exige mTLS; sandbox do Itau aceita sem certificado.
                if (dados.Ambiente == DadosBancariosEmpresa.AmbienteProducao)
                    throw new ItauPixException("Certificado do Itaú não cadastrado nos dados bancários da empresa.");
                return null;
            }

            var chavePrivada = _criptografia.Descriptografar(dados.ChavePrivadaCriptografada);
            using var doPem = X509Certificate2.CreateFromPem(dados.CertificadoPem, chavePrivada);

            // No Windows a chave carregada do PEM e efemera e o SChannel nao consegue usa-la no
            // handshake; reimportar via PKCS#12 resolve la e nao muda nada no Linux (Render).
            return new X509Certificate2(doPem.Export(X509ContentType.Pkcs12));
        }

        // Inclui ambiente, client id e o certificado: trocar qualquer um nos dados bancarios gera
        // um cliente/token novo em vez de reaproveitar o antigo.
        private static string ChaveDoCliente(DadosBancariosEmpresa dados)
        {
            var impressao = string.IsNullOrEmpty(dados.CertificadoPem)
                ? "sem-certificado"
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dados.CertificadoPem)))[..16];
            return $"{dados.EmpresaId}:{dados.Ambiente}:{dados.ClientId}:{impressao}";
        }

        private string UrlToken(DadosBancariosEmpresa dados) =>
            dados.Ambiente == DadosBancariosEmpresa.AmbienteProducao ? _config.UrlTokenProducao : _config.UrlTokenSandbox;

        private string UrlApi(DadosBancariosEmpresa dados) =>
            (dados.Ambiente == DadosBancariosEmpresa.AmbienteProducao ? _config.UrlApiProducao : _config.UrlApiSandbox).TrimEnd('/');

        // Erros da API Pix seguem o RFC 7807 (title/detail/violacoes).
        private static string MensagemDoItau(string conteudo, int status)
        {
            try
            {
                var json = JObject.Parse(conteudo);
                var detalhe = json["detail"]?.ToString() ?? json["title"]?.ToString() ?? json["message"]?.ToString();
                var violacoes = json["violacoes"] as JArray;
                if (violacoes != null && violacoes.Count > 0)
                    detalhe = $"{detalhe} ({violacoes[0]?["razao"]})";
                if (!string.IsNullOrWhiteSpace(detalhe)) return detalhe;
            }
            catch (JsonException)
            {
            }

            return $"HTTP {status}";
        }
    }
}
