using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // OAuth (authorization code) do app da Contact Solution no Mercado Pago: a empresa autoriza e a
    // plataforma passa a criar orders na conta dela, ficando com o marketplace_fee.
    public class MercadoPagoOAuthService : IMercadoPagoOAuth
    {
        private readonly MercadoPagoConfiguration _config;
        private readonly ILogger<MercadoPagoOAuthService> _logger;
        private readonly HttpClient _cliente = new() { Timeout = TimeSpan.FromSeconds(30) };

        public MercadoPagoOAuthService(IOptions<MercadoPagoConfiguration> options, ILogger<MercadoPagoOAuthService> logger)
        {
            _config = options.Value;
            _logger = logger;
        }

        public bool Configurado => _config.OAuthConfigurado;

        public string? UserIdDaPlataforma => string.IsNullOrWhiteSpace(_config.UserId) ? null : _config.UserId.Trim();

        public string UrlAutorizacao(string state) =>
            $"{_config.UrlAutorizacao}?client_id={Uri.EscapeDataString(_config.ClientId ?? string.Empty)}" +
            $"&response_type=code&platform_id=mp&state={Uri.EscapeDataString(state)}" +
            $"&redirect_uri={Uri.EscapeDataString(_config.UrlRetornoOAuth)}";

        public Task<TokensMercadoPago> TrocarCodigoAsync(string code) => PedirTokenAsync(new JObject
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _config.UrlRetornoOAuth
        }, "conectar a conta Mercado Pago");

        public Task<TokensMercadoPago> RenovarAsync(string refreshToken) => PedirTokenAsync(new JObject
        {
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        }, "renovar a autorização do Mercado Pago");

        private async Task<TokensMercadoPago> PedirTokenAsync(JObject corpo, string operacao)
        {
            if (!Configurado)
                throw new PixException("O app do Mercado Pago não está configurado no servidor (MercadoPago:ClientId e ClientSecret). Avise o suporte.");

            HttpResponseMessage resposta;
            try
            {
                resposta = await _cliente.PostAsync(_config.UrlApi.TrimEnd('/') + "/oauth/token",
                    new StringContent(corpo.ToString(Formatting.None), Encoding.UTF8, "application/json"));
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                _logger.LogWarning(ex, "MercadoPago OAuth: falha de rede ao {Operacao}", operacao);
                throw new PixException($"Não foi possível falar com o Mercado Pago para {operacao}. Tente novamente em instantes.", null, ex);
            }

            using (resposta)
            {
                var conteudo = await resposta.Content.ReadAsStringAsync();
                if (!resposta.IsSuccessStatusCode)
                {
                    // O corpo de erro do /oauth/token nao traz token; pode ir para o log.
                    _logger.LogWarning("MercadoPago OAuth: HTTP {Status} ao {Operacao}: {Corpo}", (int)resposta.StatusCode, operacao, conteudo);
                    throw new PixException($"O Mercado Pago recusou {operacao}: {Mensagem(conteudo, (int)resposta.StatusCode)}", (int)resposta.StatusCode);
                }

                JObject json;
                try { json = JObject.Parse(conteudo); }
                catch (JsonException) { throw new PixException($"Resposta inesperada do Mercado Pago ao {operacao}."); }

                var accessToken = json["access_token"]?.ToString();
                var refreshToken = json["refresh_token"]?.ToString();
                var userId = json["user_id"]?.ToString();
                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(userId))
                    throw new PixException($"O Mercado Pago não devolveu a autorização completa ao {operacao}.");

                // expires_in em segundos (180 dias); sem ele, assume o prazo documentado.
                var segundos = json["expires_in"]?.Value<long?>() ?? (long)TimeSpan.FromDays(180).TotalSeconds;
                var expiraEm = Dominio.Servicos.HorarioBrasilia.Agora().AddSeconds(segundos);

                return new TokensMercadoPago(accessToken, refreshToken, userId, expiraEm);
            }
        }

        private static string Mensagem(string conteudo, int status)
        {
            try
            {
                var json = JObject.Parse(conteudo);
                var mensagem = json["message"]?.ToString() ?? json["error_description"]?.ToString() ?? json["error"]?.ToString();
                if (!string.IsNullOrWhiteSpace(mensagem)) return mensagem;
            }
            catch (JsonException)
            {
            }

            return $"HTTP {status}";
        }
    }
}
