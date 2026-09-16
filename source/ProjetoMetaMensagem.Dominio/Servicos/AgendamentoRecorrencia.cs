using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Calculo da proxima execucao de um Agendamento. Usado tanto na criacao/edicao (define a
    // 1a ProximaExecucao a partir de DataInicio) quanto pelo AgendamentoDispatchJob (calcula a
    // proxima apos cada disparo).
    public static class AgendamentoRecorrencia
    {
        public static DateTime CalcularProximaExecucao(DateTime dataInicio, DateTime referencia, string tipoRecorrencia)
        {
            switch (tipoRecorrencia)
            {
                case Agendamento.Diaria:
                    return referencia.AddDays(1);

                case Agendamento.Semanal:
                    return referencia.AddDays(7);

                case Agendamento.Mensal:
                    // Ancorado no dia/hora de DataInicio, nunca no dia ja clampado da
                    // referencia anterior -- senao um agendamento criado no dia 31 encolheria
                    // pra sempre depois de cair num fevereiro (31 -> 28 -> 28 -> 28...).
                    var proximoMes = referencia.AddMonths(1);
                    var ultimoDiaDoMes = DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month);
                    var dia = Math.Min(dataInicio.Day, ultimoDiaDoMes);
                    return new DateTime(proximoMes.Year, proximoMes.Month, dia,
                        dataInicio.Hour, dataInicio.Minute, dataInicio.Second);

                default:
                    throw new ArgumentException($"Tipo de recorrência inválido: {tipoRecorrencia}", nameof(tipoRecorrencia));
            }
        }
    }
}
