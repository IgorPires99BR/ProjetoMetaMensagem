using System.Security.Cryptography;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    // Valida a assinatura HMAC-SHA256 que a Meta manda no header X-Hub-Signature-256 em todo
    // webhook. Sem isso, POST /api/webhook/whatsapp e AllowAnonymous e aceita qualquer payload
    // sem nenhuma credencial: um atacante conseguia injetar mensagem falsa (ou status de
    // entrega falso) em qualquer empresa, bastando reproduzir o formato do payload da Meta.
    //
    // Roda como middleware, nao como action filter, porque precisa ler o corpo cru ANTES do
    // model binding do [FromBody] consumir o stream da requisicao.
    public class ValidacaoAssinaturaMetaMiddleware
    {
        private const string RotaWebhookMeta = "/api/webhook/whatsapp";
        private const string PrefixoAssinatura = "sha256=";

        private readonly RequestDelegate _next;
        private readonly ILogger<ValidacaoAssinaturaMetaMiddleware> _logger;

        public ValidacaoAssinaturaMetaMiddleware(RequestDelegate next, ILogger<ValidacaoAssinaturaMetaMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var ehWebhookDaMeta = HttpMethods.IsPost(context.Request.Method)
                && context.Request.Path.StartsWithSegments(RotaWebhookMeta);

            if (!ehWebhookDaMeta)
            {
                await _next(context);
                return;
            }

            var appSecret = configuration["ApiWhatsappConnectionConfiguration:AppSecret"];
            if (string.IsNullOrEmpty(appSecret))
            {
                // Sem o AppSecret configurado nao ha como validar. Loga alto e deixa passar
                // pra nao derrubar o recebimento de mensagem por falta de configuracao -- mas
                // isso PRECISA ser corrigido configurando a variavel de ambiente em producao.
                _logger.LogWarning("Webhook da Meta recebido sem ApiWhatsappConnectionConfiguration:AppSecret configurado: assinatura NAO esta sendo validada.");
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();
            string corpo;
            using (var leitor = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
            {
                corpo = await leitor.ReadToEndAsync();
            }
            context.Request.Body.Position = 0;

            var assinaturaRecebida = context.Request.Headers["X-Hub-Signature-256"].ToString();

            if (!AssinaturaValida(corpo, assinaturaRecebida, appSecret))
            {
                _logger.LogWarning("Webhook da Meta descartado: assinatura invalida ou ausente. IP de origem: {Ip}", context.Connection.RemoteIpAddress);
                // 200 proposital: a Meta so chega aqui com assinatura valida, entao isto nunca
                // e um retry legitimo dela -- e so descarta sem processar o payload.
                context.Response.StatusCode = StatusCodes.Status200OK;
                return;
            }

            await _next(context);
        }

        private static bool AssinaturaValida(string corpo, string assinaturaRecebida, string appSecret)
        {
            if (string.IsNullOrEmpty(assinaturaRecebida) || !assinaturaRecebida.StartsWith(PrefixoAssinatura))
                return false;

            byte[] hashRecebido;
            try
            {
                hashRecebido = Convert.FromHexString(assinaturaRecebida.Substring(PrefixoAssinatura.Length));
            }
            catch (FormatException)
            {
                return false;
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
            var hashEsperado = hmac.ComputeHash(Encoding.UTF8.GetBytes(corpo));

            return hashEsperado.Length == hashRecebido.Length
                && CryptographicOperations.FixedTimeEquals(hashEsperado, hashRecebido);
        }
    }
}
