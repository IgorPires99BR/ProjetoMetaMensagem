using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Cada agendamento virou um job recorrente proprio no HangFire. O cron so decide quando o job
// acorda; a ProximaExecucao decide se dispara. Estes testes travam os dois lados: o cron tem
// que acordar em todo horario em que a ProximaExecucao pode cair, e a ProximaExecucao nao pode
// ficar presa no passado (senao todo acordar do cron reenviaria).
public class RecorrenciaDoAgendamentoTeste
{
    // Sabado, 03/10/2026 08:30.
    private static readonly DateTime Referencia = new(2026, 10, 3, 8, 30, 0);

    private static Agendamento NovoAgendamento(string tipo, List<int>? diasSemana = null, int? diaDoMes = null) => new()
    {
        TipoRecorrencia = tipo,
        DataReferencia = Referencia,
        ProximaExecucao = Referencia,
        DiasSemanaLista = diasSemana ?? new List<int>(),
        DiaDoMes = diaDoMes,
    };

    [Theory]
    [InlineData(Agendamento.Diaria, "30 8 * * *")]
    [InlineData(Agendamento.VencimentoContato, "30 8 * * *")]
    public void Diaria_e_vencimento_acordam_todo_dia_no_horario_da_referencia(string tipo, string cron)
    {
        Assert.Equal(cron, AgendamentoRecorrencia.ExpressaoCron(NovoAgendamento(tipo)));
    }

    [Fact]
    public void Semanal_usa_os_dias_escolhidos()
    {
        var agendamento = NovoAgendamento(Agendamento.Semanal, new List<int> { 5, 1, 3 });

        Assert.Equal("30 8 * * 1,3,5", AgendamentoRecorrencia.ExpressaoCron(agendamento));
    }

    [Fact]
    public void Semanal_sem_dias_escolhidos_usa_o_dia_da_semana_da_referencia()
    {
        Assert.Equal("30 8 * * 6", AgendamentoRecorrencia.ExpressaoCron(NovoAgendamento(Agendamento.Semanal)));
    }

    [Fact]
    public void Mensal_ate_dia_28_acorda_so_no_dia()
    {
        Assert.Equal("30 8 15 * *", AgendamentoRecorrencia.ExpressaoCron(NovoAgendamento(Agendamento.Mensal, diaDoMes: 15)));
    }

    [Fact]
    public void Mensal_depois_do_dia_28_acorda_no_fim_do_mes_pra_cobrir_o_clamp()
    {
        // Dia 31 em novembro sai no dia 30 -- um cron "31" nunca acordaria em novembro.
        Assert.Equal("30 8 28-31 * *", AgendamentoRecorrencia.ExpressaoCron(NovoAgendamento(Agendamento.Mensal, diaDoMes: 31)));
    }

    [Fact]
    public void Proxima_execucao_pula_os_ciclos_que_ja_passaram()
    {
        var agendamento = NovoAgendamento(Agendamento.Diaria);
        var dezDiasDepois = Referencia.AddDays(10).AddHours(1);

        var proxima = AgendamentoRecorrencia.CalcularProximaExecucaoFutura(agendamento, dezDiasDepois);

        Assert.Equal(new DateTime(2026, 10, 14, 8, 30, 0), proxima);
    }

    [Fact]
    public void Proxima_execucao_mensal_mantem_o_clamp_de_fim_de_mes()
    {
        var agendamento = NovoAgendamento(Agendamento.Mensal, diaDoMes: 31);
        agendamento.ProximaExecucao = new DateTime(2026, 10, 31, 8, 30, 0);

        var proxima = AgendamentoRecorrencia.CalcularProximaExecucaoFutura(agendamento, agendamento.ProximaExecucao);

        Assert.Equal(new DateTime(2026, 11, 30, 8, 30, 0), proxima);
    }
}
