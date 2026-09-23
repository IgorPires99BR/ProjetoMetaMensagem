using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// CobrancaClienteFactory monta a cobranca aberta a cada disparo de Template com
// GeraCobranca = true (ver EnviarMensagemTemplateMetaHandler/...Lote). Estes testes travam o
// snapshot (Valor/DataVencimento vem do Contato no momento do disparo, nao mudam depois) e o
// comportamento sem cadastro financeiro preenchido.
public class CobrancaClienteFactoryTeste
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid TemplateId = Guid.NewGuid();
    private static readonly Guid HistoricoDisparoId = Guid.NewGuid();
    private static readonly DateTime Setembro2026 = new(2026, 9, 5);

    private static Contato ContatoDeExemplo(int? diaVencimento = 20, decimal? valorFatura = 405m) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = EmpresaId,
        Telefone = "5511999990000",
        NomeContato = "Maria",
        NomeCliente = "Maria Souza",
        DiaVencimento = diaVencimento,
        ValorFatura = valorFatura,
    };

    [Fact]
    public void Snapshot_usa_valor_e_vencimento_do_contato_no_momento_do_disparo()
    {
        var contato = ContatoDeExemplo(diaVencimento: 20, valorFatura: 405m);

        var cobranca = CobrancaClienteFactory.Criar(contato, TemplateId, HistoricoDisparoId, Setembro2026);

        Assert.Equal(EmpresaId, cobranca.EmpresaId);
        Assert.Equal(contato.Id, cobranca.ContatoId);
        Assert.Equal(TemplateId, cobranca.TemplateId);
        Assert.Equal(HistoricoDisparoId, cobranca.HistoricoDisparoId);
        Assert.Equal(405m, cobranca.Valor);
        Assert.Equal(new DateTime(2026, 9, 20), cobranca.DataVencimento);
        Assert.Equal(StatusCobrancaCliente.Pendente, cobranca.Status);
    }

    [Fact]
    public void Vencimento_e_clampado_pro_ultimo_dia_do_mes_igual_ao_ResolvedorDeVariaveis()
    {
        var contato = ContatoDeExemplo(diaVencimento: 31);
        var fevereiro2026 = new DateTime(2026, 2, 10);

        var cobranca = CobrancaClienteFactory.Criar(contato, TemplateId, HistoricoDisparoId, fevereiro2026);

        Assert.Equal(new DateTime(2026, 2, 28), cobranca.DataVencimento);
    }

    [Fact]
    public void Contato_sem_cadastro_financeiro_abre_cobranca_com_valor_zero_e_vencimento_hoje()
    {
        var contato = ContatoDeExemplo(diaVencimento: null, valorFatura: null);

        var cobranca = CobrancaClienteFactory.Criar(contato, TemplateId, HistoricoDisparoId, Setembro2026);

        Assert.Equal(0m, cobranca.Valor);
        Assert.Equal(Setembro2026.Date, cobranca.DataVencimento);
    }
}
