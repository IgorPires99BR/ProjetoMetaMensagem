using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Regras do Pix da cobranca decididas com o Igor: multa unica (TaxaJuros %) + juros pro rata
// dia (TaxaJurosMensal % / 30) so depois do vencimento; Pix vale ate 23:59:59 do vencimento,
// ou do dia do envio se ja saiu vencido. A variavel valorAtualizado tem que bater com o Pix.
public class CobrancaPixTeste
{
    private static readonly DateTime Vencimento = new(2026, 9, 20);

    [Fact]
    public void Antes_do_vencimento_nao_tem_encargo_e_Pix_vale_ate_o_fim_do_vencimento()
    {
        var r = CalculoCobrancaPix.Calcular(405m, 2m, 2m, Vencimento, new DateTime(2026, 9, 10, 14, 30, 0));

        Assert.Equal(0m, r.Multa);
        Assert.Equal(0m, r.Juros);
        Assert.Equal(405m, r.ValorCobrado);
        Assert.Equal(new DateTime(2026, 9, 20, 23, 59, 59), r.ExpiraEm);
    }

    [Fact]
    public void No_dia_do_vencimento_ainda_nao_esta_vencido()
    {
        var r = CalculoCobrancaPix.Calcular(405m, 2m, 2m, Vencimento, new DateTime(2026, 9, 20, 22, 0, 0));

        Assert.Equal(0, r.DiasAtraso);
        Assert.Equal(405m, r.ValorCobrado);
    }

    [Fact]
    public void Vencido_cobra_multa_unica_e_juros_por_dia_e_Pix_vale_ate_o_fim_do_dia_do_envio()
    {
        // 10 dias de atraso: multa 2% de 405 = 8,10; juros 2%/30 * 10 dias = 2,70.
        var r = CalculoCobrancaPix.Calcular(405m, 2m, 2m, Vencimento, new DateTime(2026, 9, 30, 9, 0, 0));

        Assert.Equal(10, r.DiasAtraso);
        Assert.Equal(8.10m, r.Multa);
        Assert.Equal(2.70m, r.Juros);
        Assert.Equal(415.80m, r.ValorCobrado);
        Assert.Equal(new DateTime(2026, 9, 30, 23, 59, 59), r.ExpiraEm);
    }

    [Fact]
    public void Taxas_nulas_nao_geram_encargo()
    {
        var r = CalculoCobrancaPix.Calcular(100m, null, null, Vencimento, new DateTime(2026, 10, 5));

        Assert.Equal(100m, r.ValorCobrado);
    }

    [Fact]
    public void Variavel_valorAtualizado_bate_com_o_valor_do_Pix()
    {
        var contato = new Contato { Id = Guid.NewGuid(), Telefone = "5511999990000", DiaVencimento = 20, ValorFatura = 405m, TaxaJuros = 2m, TaxaJurosMensal = 2m };
        var hoje = new DateTime(2026, 9, 30, 9, 0, 0);

        var texto = ResolvedorDeVariaveis.Resolver(
            new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.ValorAtualizado }, contato, new Dictionary<Guid, Parametro>(), hoje);

        Assert.Equal("R$ 415,80", texto);
    }

    [Fact]
    public void Variaveis_de_Pix_viram_marcadores_trocados_depois_pelo_Pix_gerado()
    {
        var contato = new Contato { Id = Guid.NewGuid(), Telefone = "5511999990000", DiaVencimento = 20, ValorFatura = 405m };
        var parametros = new Dictionary<Guid, Parametro>();
        var hoje = new DateTime(2026, 9, 10);

        var valores = new List<string>
        {
            ResolvedorDeVariaveis.Resolver(new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.PixCopiaECola }, contato, parametros, hoje),
            ResolvedorDeVariaveis.Resolver(new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.LinkPagamento }, contato, parametros, hoje),
            "texto fixo"
        };

        var cobranca = new CobrancaCliente { Txid = "abc", PixCopiaECola = "00020126PIX" };
        var preparada = new CobrancaPreparada { Cobranca = cobranca, LinkPagamento = "https://x/pix/abc" };
        var erro = new PreparadorCobrancaPix(null!, null!, null!).AplicarNasVariaveis(valores, preparada);

        Assert.Null(erro);
        Assert.Equal(new[] { "00020126PIX", "https://x/pix/abc", "texto fixo" }, valores);
    }

    [Fact]
    public void Variavel_de_Pix_sem_Pix_gerado_impede_o_envio()
    {
        var valores = new List<string> { ResolvedorDeVariaveis.MarcadorPixCopiaECola };
        var semPix = new CobrancaPreparada { Cobranca = new CobrancaCliente() };

        var erro = new PreparadorCobrancaPix(null!, null!, null!).AplicarNasVariaveis(valores, semPix);

        Assert.NotNull(erro);
        Assert.Equal(ResolvedorDeVariaveis.MarcadorPixCopiaECola, valores[0]);
    }

    [Fact]
    public void Txid_e_o_Id_da_cobranca_sem_hifens_no_formato_do_Bacen()
    {
        var id = Guid.NewGuid();

        var txid = CobrancaCliente.TxidDe(id);

        Assert.Matches("^[a-f0-9]{32}$", txid);
    }
}
