using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Servico.Cobranca;
using ProjetoMetaMensagem.Servico.Configuration;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Pix pelo Asaas: o pagador precisa de CPF/CNPJ valido e o split fixo para a subconta da empresa
// nao pode passar do liquido (valor - taxa do Asaas), senao o Asaas bloqueia o repasse DEPOIS
// que o cliente ja pagou.
public class CobrancaAsaasTeste
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("11.222.333/0001-81")]
    public void Documento_com_digito_verificador_certo_e_aceito(string documento)
    {
        Assert.True(DocumentoHelper.CpfCnpjValido(documento));
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("111.111.111-11")]
    [InlineData("11.222.333/0001-80")]
    [InlineData("1234")]
    [InlineData("")]
    [InlineData(null)]
    public void Documento_invalido_e_recusado(string? documento)
    {
        Assert.False(DocumentoHelper.CpfCnpjValido(documento));
    }

    [Fact]
    public void Contato_grava_CpfCnpj_so_com_digitos()
    {
        var contato = new Contato(new CriaContatoCommand { Telefone = "11999998888", CpfCnpj = "529.982.247-25" });

        Assert.Equal("52998224725", contato.CpfCnpj);
    }

    [Fact]
    public void Contato_com_CpfCnpj_invalido_nao_passa_na_validacao()
    {
        var resultado = new CriaContatoValidator().Validate(new CriaContatoCommand
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Telefone = "11999998888",
            CpfCnpj = "123.456.789-00"
        });

        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriaContatoCommand.CpfCnpj));
    }

    [Fact]
    public async Task Comissao_menor_que_a_taxa_do_Asaas_nao_gera_Pix()
    {
        // Sem ApiKey: se o servico chegasse a chamar o Asaas, o erro seria outro.
        var servico = new AsaasPixService(Options.Create(new AsaasConfiguration { TaxaPix = 1.99m }), NullLogger<AsaasPixService>.Instance);
        var cobranca = new NovaCobrancaPix("abc", 100m, DateTime.Today, null,
            new PagadorPix("Fulano", "52998224725", null), "wallet", ValorRepasse: 99m);

        var erro = await Assert.ThrowsAsync<PixException>(() => servico.CriarCobrancaAsync(cobranca));

        Assert.Contains("taxa do Asaas", erro.Message);
    }

    [Fact]
    public void Comissao_percentual_sai_da_parte_da_empresa()
    {
        var r = CalculoComissao.Calcular(405m, DadosBancariosEmpresa.ComissaoPercentual, 3m)!;

        Assert.Equal(12.15m, r.Comissao);
        Assert.Equal(392.85m, r.Repasse);
    }

    [Fact]
    public void Pix_so_conta_como_pago_quando_o_Asaas_diz_RECEIVED()
    {
        // CONFIRMED no Pix e o bloqueio cautelar de ate 72h: dinheiro ainda nao disponivel.
        Assert.True(new ConsultaCobrancaPix("RECEIVED", 10m, DateTime.Now, null).Paga);
        Assert.False(new ConsultaCobrancaPix("CONFIRMED", 10m, DateTime.Now, null).Paga);
        Assert.False(new ConsultaCobrancaPix("PENDING", null, null, null).Paga);
    }

    [Fact]
    public async Task Situacao_da_subconta_e_consultada_com_a_chave_da_subconta()
    {
        // Com a chave da plataforma o /myAccount devolveria a situacao da propria plataforma.
        var livre = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        livre.Start();
        var porta = ((System.Net.IPEndPoint)livre.LocalEndpoint).Port;
        livre.Stop();
        using var servidor = new System.Net.HttpListener();
        servidor.Prefixes.Add($"http://localhost:{porta}/");
        servidor.Start();

        var chavesRecebidas = new List<string?>();
        var atendimento = Task.Run(async () =>
        {
            for (var i = 0; i < 2; i++)
            {
                var ctx = await servidor.GetContextAsync();
                chavesRecebidas.Add(ctx.Request.Headers["access_token"]);
                var json = ctx.Request.Url!.AbsolutePath.EndsWith("/status")
                    ? """{"id":"acc_1","commercialInfo":"APPROVED","bankAccountInfo":"APPROVED","documentation":"PENDING","general":"PENDING"}"""
                    : """{"rejectReasons":null,"data":[{"id":"g1","title":"Documentos de identificação","description":"RG ou CNH","status":"NOT_SENT","type":"IDENTIFICATION","responsible":{"name":"Gilson","type":"PARTNER"},"onboardingUrl":"https://asaas.com/onboarding/abc","onboardingUrlExpirationDate":"2026-10-21 10:00:00","documents":[]}]}""";
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                ctx.Response.ContentType = "application/json";
                await ctx.Response.OutputStream.WriteAsync(bytes);
                ctx.Response.Close();
            }
        });

        var servico = new AsaasPixService(Options.Create(new AsaasConfiguration
        {
            ApiKey = "$aact_plataforma",
            UrlApiSandbox = $"http://localhost:{porta}/v3"
        }), NullLogger<AsaasPixService>.Instance);

        var situacao = await servico.ConsultarSituacaoSubcontaAsync("$aact_subconta");
        await atendimento;

        Assert.All(chavesRecebidas, c => Assert.Equal("$aact_subconta", c));
        Assert.Equal("PENDING", situacao.Geral);
        Assert.Equal("PENDING", situacao.Documentacao);
        var documento = Assert.Single(situacao.Documentos);
        Assert.Equal("NOT_SENT", documento.Status);
        Assert.Equal("https://asaas.com/onboarding/abc", documento.LinkEnvio);
        Assert.Equal("2026-10-21 10:00:00", documento.LinkExpiraEm);
    }

    // Chave de um ambiente com Asaas:Ambiente do outro so volta 401 do Asaas, sem motivo.
    [Theory]
    [InlineData("PRODUCAO", "$aact_hmlg_abc", true)]
    [InlineData("SANDBOX", "$aact_prod_abc", true)]
    [InlineData("producao", "$aact_prod_abc", false)]
    [InlineData("SANDBOX", "$aact_hmlg_abc", false)]
    [InlineData("SANDBOX", "$aact_YTU5YTE0M2M2N2I4MTliNzk0YTI5N2U5MzdjNWZmNDQ6", false)] // formato antigo: nao barra
    public void Chave_do_ambiente_errado_e_apontada(string ambiente, string chave, bool erro)
    {
        var config = new AsaasConfiguration { Ambiente = ambiente, ApiKey = chave };

        Assert.Equal(erro, config.ErroChaveDoAmbiente() != null);
    }

    [Fact]
    public async Task Chave_do_ambiente_errado_falha_antes_de_chamar_o_Asaas()
    {
        var servico = new AsaasPixService(Options.Create(new AsaasConfiguration { Ambiente = "PRODUCAO", ApiKey = "$aact_hmlg_abc" }),
            NullLogger<AsaasPixService>.Instance);

        var erro = await Assert.ThrowsAsync<PixException>(() => servico.TestarCredenciaisAsync());

        Assert.Contains("SANDBOX", erro.Message);
    }

    [Fact]
    public async Task Simular_pagamento_em_producao_e_barrado_no_servico()
    {
        var servico = new AsaasPixService(Options.Create(new AsaasConfiguration { Ambiente = "PRODUCAO", ApiKey = "$aact_prod_abc" }),
            NullLogger<AsaasPixService>.Instance);

        Assert.False(servico.Sandbox);
        await Assert.ThrowsAsync<PixException>(() => servico.ConfirmarPagamentoSandboxAsync("pay_123"));
    }
}
