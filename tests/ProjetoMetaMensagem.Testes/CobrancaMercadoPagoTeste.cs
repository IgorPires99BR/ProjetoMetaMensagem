using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Cobranca;
using ProjetoMetaMensagem.Servico.Configuration;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Pix pela API de Orders do Mercado Pago, contra um servidor HTTP local que faz o papel da API:
// confere o que vai no POST /v1/orders e como a resposta vira cobranca, baixa e cancelamento.
public class CobrancaMercadoPagoTeste
{
    [Fact]
    public async Task Order_Pix_vai_com_external_reference_idempotencia_e_sem_split()
    {
        using var api = new ApiFalsa("""
            {"id":"ORD01ABC","status":"action_required","status_detail":"waiting_transfer",
             "transactions":{"payments":[{"id":"PAY01","payment_method":{"id":"pix","type":"bank_transfer",
               "qr_code":"00020126580014br.gov.bcb.pix","ticket_url":"https://www.mercadopago.com.br/payments/1/ticket"}}]}}
            """);
        var servico = api.Servico();

        var pix = await servico.CriarCobrancaAsync(new NovaCobrancaPix("txid123", 405.5m, DateTime.Today.AddDays(3), "Fatura",
            new PagadorPix("Maria", "52998224725", "maria@exemplo.com"), null, null));

        Assert.Equal("ORD01ABC", pix.IdExterno);
        Assert.Equal("00020126580014br.gov.bcb.pix", pix.PixCopiaECola);
        Assert.Equal("https://www.mercadopago.com.br/payments/1/ticket", pix.LinkExterno);

        var (metodo, caminho, cabecalhos, corpo) = Assert.Single(api.Recebidas);
        Assert.Equal("POST", metodo);
        Assert.Equal("/v1/orders", caminho);
        Assert.Equal("Bearer APP_USR-teste", cabecalhos["Authorization"]);
        Assert.Equal("txid123", cabecalhos["X-Idempotency-Key"]);

        var json = JObject.Parse(corpo!);
        Assert.Equal("online", json["type"]!.ToString());
        Assert.Equal("automatic", json["processing_mode"]!.ToString());
        Assert.Equal("txid123", json["external_reference"]!.ToString());
        Assert.Equal("405.50", json["total_amount"]!.ToString());
        Assert.Equal("maria@exemplo.com", json["payer"]!["email"]!.ToString());
        Assert.Equal("CPF", json["payer"]!["identification"]!["type"]!.ToString());
        var pagamento = json["transactions"]!["payments"]![0]!;
        Assert.Equal("pix", pagamento["payment_method"]!["id"]!.ToString());
        Assert.Equal("bank_transfer", pagamento["payment_method"]!["type"]!.ToString());
        Assert.StartsWith("PT", pagamento["expiration_time"]!.ToString());
        Assert.Null(json["split"]);
    }

    [Fact]
    public async Task Contato_sem_email_usa_o_email_padrao_e_sem_documento_nao_manda_identificacao()
    {
        using var api = new ApiFalsa("""{"id":"ORD1","status":"action_required","transactions":{"payments":[{"payment_method":{"qr_code":"000201"}}]}}""");

        await api.Servico(emailPadrao: "cobranca@contactsolution.com.br").CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", null), null, null));

        var json = JObject.Parse(api.Recebidas.Single().Corpo!);
        Assert.Equal("cobranca@contactsolution.com.br", json["payer"]!["email"]!.ToString());
        Assert.Null(json["payer"]!["identification"]);
    }

    [Fact]
    public async Task Sem_email_nenhum_a_order_nem_e_pedida()
    {
        using var api = new ApiFalsa("{}");

        var erro = await Assert.ThrowsAsync<PixException>(() => api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", null), null, null)));

        Assert.Contains("e-mail", erro.Message);
        Assert.Empty(api.Recebidas);
    }

    [Fact]
    public async Task Repasse_para_subconta_e_recusado_no_Mercado_Pago()
    {
        using var api = new ApiFalsa("{}");

        await Assert.ThrowsAsync<PixException>(() => api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", "maria@exemplo.com"), "wallet", 9m)));
        Assert.Empty(api.Recebidas);
    }

    // --- Cobranca na conta Mercado Pago da empresa (OAuth), com a comissao da plataforma ---

    [Fact]
    public async Task Order_na_conta_da_empresa_vai_com_o_token_dela_e_a_comissao_no_marketplace_fee()
    {
        using var api = new ApiFalsa("""{"id":"ORD1","status":"action_required","transactions":{"payments":[{"payment_method":{"qr_code":"000201"}}]}}""");

        await api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("txid9", 405m, DateTime.Today, null,
            new PagadorPix("Maria", "", "maria@exemplo.com"), null, null, TokenConta: "APP_USR-da-empresa", Comissao: 40.5m));

        var (_, _, cabecalhos, corpo) = api.Recebidas.Single();
        Assert.Equal("Bearer APP_USR-da-empresa", cabecalhos["Authorization"]);
        var json = JObject.Parse(corpo!);
        Assert.Equal("40.50", json["marketplace_fee"]!.ToString());
        Assert.Equal("405.00", json["total_amount"]!.ToString());
    }

    [Fact]
    public async Task Order_da_Contact_Solution_nao_leva_marketplace_fee()
    {
        using var api = new ApiFalsa("""{"id":"ORD1","status":"action_required","transactions":{"payments":[{"payment_method":{"qr_code":"000201"}}]}}""");

        await api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", "maria@exemplo.com"), null, null));

        var (_, _, cabecalhos, corpo) = api.Recebidas.Single();
        Assert.Equal("Bearer APP_USR-teste", cabecalhos["Authorization"]);
        Assert.Null(JObject.Parse(corpo!)["marketplace_fee"]);
    }

    [Theory]
    [InlineData(null, 5.0)]                   // comissao sem a conta da empresa: cairia na propria plataforma
    [InlineData("APP_USR-da-empresa", null)]  // conta da empresa sem comissao: a plataforma nao receberia nada
    [InlineData("APP_USR-da-empresa", 10.0)]  // comissao igual ao valor: nada para a empresa
    public async Task Comissao_inconsistente_nem_chega_ao_Mercado_Pago(string? token, double? comissao)
    {
        using var api = new ApiFalsa("{}");

        await Assert.ThrowsAsync<PixException>(() => api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", "maria@exemplo.com"), null, null, token, (decimal?)comissao)));
        Assert.Empty(api.Recebidas);
    }

    [Fact]
    public async Task Consulta_de_order_da_empresa_usa_o_token_dela()
    {
        using var api = new ApiFalsa("""{"id":"ORD1","status":"action_required","status_detail":"waiting_transfer"}""");

        await api.Servico().ConsultarCobrancaAsync("ORD1", "APP_USR-da-empresa");

        Assert.Equal("Bearer APP_USR-da-empresa", api.Recebidas.Single().Cabecalhos["Authorization"]);
    }

    // --- OAuth da conta da empresa ---

    [Fact]
    public void Link_de_autorizacao_leva_client_id_state_e_a_url_de_retorno_cadastrada()
    {
        var oauth = new MercadoPagoOAuthService(Options.Create(new MercadoPagoConfiguration
        {
            ClientId = "2725", ClientSecret = "segredo", UrlRetornoOAuth = "https://api.exemplo.com/api/mercadopago/oauth/retorno"
        }), NullLogger<MercadoPagoOAuthService>.Instance);

        var url = new Uri(oauth.UrlAutorizacao("estado+com/simbolos="));
        var query = System.Web.HttpUtility.ParseQueryString(url.Query);

        Assert.Equal("auth.mercadopago.com", url.Host);
        Assert.Equal("2725", query["client_id"]);
        Assert.Equal("code", query["response_type"]);
        Assert.Equal("mp", query["platform_id"]);
        Assert.Equal("estado+com/simbolos=", query["state"]);
        Assert.Equal("https://api.exemplo.com/api/mercadopago/oauth/retorno", query["redirect_uri"]);
    }

    [Fact]
    public async Task Code_do_retorno_vira_tokens_da_conta_da_empresa()
    {
        using var api = new ApiFalsa("""{"access_token":"APP_USR-novo","token_type":"bearer","expires_in":15552000,"scope":"offline_access read write","user_id":987654,"refresh_token":"TG-novo","public_key":"APP_USR-pk","live_mode":true}""");
        var oauth = api.OAuth();

        var tokens = await oauth.TrocarCodigoAsync("TG-code");

        Assert.Equal("APP_USR-novo", tokens.AccessToken);
        Assert.Equal("TG-novo", tokens.RefreshToken);
        Assert.Equal("987654", tokens.UserId);
        Assert.InRange(tokens.ExpiraEm, DateTime.Now.AddDays(179), DateTime.Now.AddDays(181));

        var (metodo, caminho, _, corpo) = api.Recebidas.Single();
        Assert.Equal("POST", metodo);
        Assert.Equal("/oauth/token", caminho);
        var json = JObject.Parse(corpo!);
        Assert.Equal("authorization_code", json["grant_type"]!.ToString());
        Assert.Equal("TG-code", json["code"]!.ToString());
        Assert.Equal("https://retorno.teste/oauth", json["redirect_uri"]!.ToString());
    }

    [Fact]
    public void State_do_OAuth_volta_a_mesma_empresa_e_vence()
    {
        var cripto = new CriptografiaDeTeste();
        var empresaId = Guid.NewGuid();
        var agora = new DateTime(2026, 10, 30, 10, 0, 0);

        var state = Dominio.Servicos.EstadoOAuthMercadoPago.Gerar(empresaId, agora, cripto);

        Assert.Equal(empresaId, Dominio.Servicos.EstadoOAuthMercadoPago.Ler(state, agora.AddMinutes(10), cripto));
        Assert.Null(Dominio.Servicos.EstadoOAuthMercadoPago.Ler(state, agora.AddHours(1), cripto));      // vencido
        Assert.Null(Dominio.Servicos.EstadoOAuthMercadoPago.Ler("lixo", agora, cripto));                // adulterado
        Assert.Null(Dominio.Servicos.EstadoOAuthMercadoPago.Ler(null, agora, cripto));
    }

    // Reversivel e que recusa texto que nao gerou (como o AES-GCM recusa um state forjado).
    private class CriptografiaDeTeste : ICriptografiaService
    {
        public bool Configurada => true;
        public string Criptografar(string textoPuro) => "ok:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(textoPuro));
        public string Descriptografar(string textoCriptografado) =>
            textoCriptografado.StartsWith("ok:")
                ? Encoding.UTF8.GetString(Convert.FromBase64String(textoCriptografado[3..]))
                : throw new CryptographicException("texto nao gerado aqui");
    }

    [Fact]
    public async Task Order_processada_e_creditada_conta_como_paga()
    {
        using var api = new ApiFalsa("""
            {"id":"ORD1","status":"processed","status_detail":"accredited","total_amount":"405.50","total_paid_amount":"405.50",
             "transactions":{"payments":[{"paid_amount":"405.50","date_approved":"2026-10-30T14:00:00.000-03:00"}]}}
            """);

        var consulta = await api.Servico().ConsultarCobrancaAsync("ORD1", null);

        Assert.True(consulta.Paga);
        Assert.Equal(405.50m, consulta.ValorPago);
        Assert.Equal(new DateTime(2026, 10, 30, 14, 0, 0), consulta.PagoEm);
        Assert.Equal("/v1/orders/ORD1", api.Recebidas.Single().Caminho);
    }

    [Theory]
    [InlineData("action_required", "waiting_transfer", true)]
    [InlineData("canceled", "expired", false)]
    [InlineData("processing", "in_process", false)]
    public async Task Order_nao_creditada_nao_conta_como_paga(string status, string detalhe, bool aguardando)
    {
        using var api = new ApiFalsa($$"""{"id":"ORD1","status":"{{status}}","status_detail":"{{detalhe}}","total_amount":"10.00"}""");

        var consulta = await api.Servico().ConsultarCobrancaAsync("ORD1", null);

        Assert.False(consulta.Paga);
        Assert.Equal(aguardando, consulta.Pendente);
    }

    [Fact]
    public async Task Cancelar_chama_o_cancel_da_order()
    {
        using var api = new ApiFalsa("""{"id":"ORD1","status":"canceled"}""");

        await api.Servico().CancelarCobrancaAsync("ORD1", null);

        var recebida = api.Recebidas.Single();
        Assert.Equal("POST", recebida.Metodo);
        Assert.Equal("/v1/orders/ORD1/cancel", recebida.Caminho);
    }

    [Fact]
    public async Task Erro_do_Mercado_Pago_vira_mensagem_legivel()
    {
        using var api = new ApiFalsa("""{"errors":[{"code":"invalid_email","message":"Invalid payer email","details":["payer.email"]}]}""", status: 400);

        var erro = await Assert.ThrowsAsync<PixException>(() => api.Servico().CriarCobrancaAsync(new NovaCobrancaPix("t", 10m, DateTime.Today, null,
            new PagadorPix("Maria", "", "maria@exemplo.com"), null, null)));

        Assert.Contains("Invalid payer email", erro.Message);
    }

    [Theory]
    [InlineData(0, 30, "PT13H29M")]   // 10:30 -> 23:59 do mesmo dia
    [InlineData(-1, 30, "PT0H31M")]   // vencido: minimo de 31 minutos do Mercado Pago
    [InlineData(60, 30, "PT720H")]    // longe demais: maximo de 30 dias
    public void Validade_do_Pix_vai_ate_o_fim_do_vencimento_dentro_dos_limites(int diasAteVencer, int minuto, string esperado)
    {
        var agora = new DateTime(2026, 10, 30, 10, minuto, 0);

        Assert.Equal(esperado, MercadoPagoOrderService.Validade(agora.Date.AddDays(diasAteVencer), agora));
    }

    // --- Assinatura do webhook ---

    private const string Segredo = "segredo-do-painel";

    private static string Assinar(string manifesto) =>
        Convert.ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(Segredo)).ComputeHash(Encoding.UTF8.GetBytes(manifesto))).ToLowerInvariant();

    [Fact]
    public void Assinatura_valida_confere_com_id_em_minusculas()
    {
        var v1 = Assinar("id:ord01abc;request-id:req-1;ts:1730300000;");

        Assert.True(AssinaturaWebhookMercadoPago.Valida($"ts=1730300000,v1={v1}", "req-1", "ORD01ABC", Segredo));
    }

    [Fact]
    public void Assinatura_sem_request_id_tira_o_trecho_do_manifesto()
    {
        var v1 = Assinar("id:ord01abc;ts:1730300000;");

        Assert.True(AssinaturaWebhookMercadoPago.Valida($"ts=1730300000,v1={v1}", null, "ORD01ABC", Segredo));
    }

    [Theory]
    [InlineData("ORD99999")]         // id trocado
    [InlineData(null)]               // sem id
    public void Assinatura_de_outra_order_e_recusada(string? dataId)
    {
        var v1 = Assinar("id:ord01abc;request-id:req-1;ts:1730300000;");

        Assert.False(AssinaturaWebhookMercadoPago.Valida($"ts=1730300000,v1={v1}", "req-1", dataId, Segredo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ts=1730300000")]
    [InlineData("v1=abc")]
    public void Cabecalho_incompleto_e_recusado(string cabecalho)
    {
        Assert.False(AssinaturaWebhookMercadoPago.Valida(cabecalho, "req-1", "ORD01ABC", Segredo));
    }

    // Servidor local no lugar de api.mercadopago.com: guarda cada requisicao e responde sempre o mesmo corpo.
    private sealed class ApiFalsa : IDisposable
    {
        private readonly System.Net.HttpListener _servidor = new();
        private readonly int _porta;

        public List<(string Metodo, string Caminho, Dictionary<string, string?> Cabecalhos, string? Corpo)> Recebidas { get; } = new();

        public ApiFalsa(string resposta, int status = 200)
        {
            var livre = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            livre.Start();
            _porta = ((System.Net.IPEndPoint)livre.LocalEndpoint).Port;
            livre.Stop();
            _servidor.Prefixes.Add($"http://localhost:{_porta}/");
            _servidor.Start();

            _ = Task.Run(async () =>
            {
                while (_servidor.IsListening)
                {
                    System.Net.HttpListenerContext ctx;
                    try { ctx = await _servidor.GetContextAsync(); }
                    catch { return; }

                    using var leitor = new StreamReader(ctx.Request.InputStream);
                    var corpo = await leitor.ReadToEndAsync();
                    lock (Recebidas)
                        Recebidas.Add((ctx.Request.HttpMethod, ctx.Request.Url!.AbsolutePath,
                            ctx.Request.Headers.AllKeys.ToDictionary(k => k!, k => ctx.Request.Headers[k]),
                            corpo == string.Empty ? null : corpo));

                    var bytes = Encoding.UTF8.GetBytes(resposta);
                    ctx.Response.StatusCode = status;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.OutputStream.WriteAsync(bytes);
                    ctx.Response.Close();
                }
            });
        }

        public MercadoPagoOrderService Servico(string? emailPadrao = null) =>
            new(Options.Create(new MercadoPagoConfiguration
            {
                AccessToken = "APP_USR-teste",
                UrlApi = $"http://localhost:{_porta}",
                EmailPagadorPadrao = emailPadrao
            }), NullLogger<MercadoPagoOrderService>.Instance);

        public MercadoPagoOAuthService OAuth() =>
            new(Options.Create(new MercadoPagoConfiguration
            {
                ClientId = "2725",
                ClientSecret = "segredo",
                UrlApi = $"http://localhost:{_porta}",
                UrlRetornoOAuth = "https://retorno.teste/oauth"
            }), NullLogger<MercadoPagoOAuthService>.Instance);

        public void Dispose() => _servidor.Close();
    }
}
