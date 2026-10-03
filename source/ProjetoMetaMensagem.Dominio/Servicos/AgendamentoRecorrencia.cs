using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Calculo da proxima execucao de um Agendamento. Usado tanto na criacao/edicao (define a
    // 1a ProximaExecucao a partir de DataReferencia) quanto pelo ProcessaAgendamentoHandler
    // (calcula a proxima apos cada disparo).
    public static class AgendamentoRecorrencia
    {
        // 1a execucao a partir da DataReferencia escolhida na tela. Sem dias da semana
        // especificos (SEMANAL "antigo"), dispara exatamente na DataReferencia. Com dias
        // escolhidos, se a DataReferencia nao cair num deles avanca pro primeiro que cair --
        // sem isto, um agendamento criado numa terca pra rodar seg/qua/sex so dispararia pela
        // primeira vez numa terca, um dia fora da recorrencia pedida.
        public static DateTime CalcularPrimeiraExecucao(DateTime dataReferencia, string tipoRecorrencia, List<int> diasSemana)
        {
            if (tipoRecorrencia != Agendamento.Semanal || diasSemana == null || diasSemana.Count == 0)
                return dataReferencia;

            if (diasSemana.Contains((int)dataReferencia.DayOfWeek))
                return dataReferencia;

            return ProximoDiaDaSemana(dataReferencia.TimeOfDay, dataReferencia.Date, diasSemana);
        }

        public static DateTime CalcularProximaExecucao(
            DateTime dataReferencia, DateTime referencia, string tipoRecorrencia,
            List<int> diasSemana = null, int? diaDoMes = null)
        {
            switch (tipoRecorrencia)
            {
                case Agendamento.Diaria:
                case Agendamento.VencimentoContato:
                    // VencimentoContato precisa ser conferido todo dia (o dia que "vence" varia
                    // por contato dentro da mesma lista) -- quem decide se dispara ou nao pra
                    // cada um e o ProcessaAgendamentoHandler, nao esta funcao.
                    return referencia.AddDays(1);

                case Agendamento.Semanal:
                    // Dias especificos (ex: seg/qua/sex): acha o proximo dentre eles, olhando os
                    // proximos 7 dias a partir da ultima execucao. Sem dias escolhidos, mantem o
                    // comportamento antigo -- sempre 7 dias depois, no mesmo dia da semana.
                    if (diasSemana != null && diasSemana.Count > 0)
                        return ProximoDiaDaSemana(dataReferencia.TimeOfDay, referencia.Date, diasSemana);
                    return referencia.AddDays(7);

                case Agendamento.Mensal:
                    // Ancorado no dia/hora escolhido (DiaDoMes, ou o dia de DataReferencia se
                    // nenhum foi informado), nunca no dia ja clampado da referencia anterior --
                    // senao um agendamento no dia 31 encolheria pra sempre depois de um fevereiro
                    // (31 -> 28 -> 28 -> 28...).
                    var proximoMes = referencia.AddMonths(1);
                    var ultimoDiaDoMes = DateTime.DaysInMonth(proximoMes.Year, proximoMes.Month);
                    var dia = Math.Min(diaDoMes ?? dataReferencia.Day, ultimoDiaDoMes);
                    return new DateTime(proximoMes.Year, proximoMes.Month, dia,
                        dataReferencia.Hour, dataReferencia.Minute, dataReferencia.Second);

                default:
                    throw new ArgumentException($"Tipo de recorrência inválido: {tipoRecorrencia}", nameof(tipoRecorrencia));
            }
        }

        // Proxima execucao estritamente depois de 'agora', pulando os ciclos que ja passaram. Sem
        // o pulo, um agendamento retomado depois de semanas pausado (ou com o servidor fora do
        // ar) ficaria com ProximaExecucao no passado e dispararia em todo ciclo ate alcancar hoje.
        public static DateTime CalcularProximaExecucaoFutura(Agendamento agendamento, DateTime agora)
        {
            var proxima = agendamento.ProximaExecucao;
            do
            {
                proxima = CalcularProximaExecucao(
                    agendamento.DataReferencia, proxima, agendamento.TipoRecorrencia,
                    agendamento.DiasSemanaLista, agendamento.DiaDoMes);
            }
            while (proxima <= agora);

            return proxima;
        }

        // Cron do job recorrente do agendamento no HangFire (minuto/hora da DataReferencia). O
        // cron so decide QUANDO o job acorda; quem decide se dispara e a ProximaExecucao (ver
        // ProcessaAgendamentoHandler). Mensal com dia 29-31 acorda em 28-31 porque cron nao sabe
        // clampar pro ultimo dia do mes (dia 31 em abril deve sair no 30) -- nos dias que nao
        // batem com a ProximaExecucao o job so acorda e volta a dormir.
        public static string ExpressaoCron(Agendamento agendamento)
        {
            var referencia = agendamento.DataReferencia;
            var minutoEHora = $"{referencia.Minute} {referencia.Hour}";

            switch (agendamento.TipoRecorrencia)
            {
                case Agendamento.Diaria:
                case Agendamento.VencimentoContato:
                    return $"{minutoEHora} * * *";

                case Agendamento.Semanal:
                    var dias = agendamento.DiasSemanaLista.Count > 0
                        ? agendamento.DiasSemanaLista
                        : new List<int> { (int)referencia.DayOfWeek };
                    return $"{minutoEHora} * * {string.Join(",", dias.Distinct().OrderBy(d => d))}";

                case Agendamento.Mensal:
                    var dia = agendamento.DiaDoMes ?? referencia.Day;
                    return dia <= 28 ? $"{minutoEHora} {dia} * *" : $"{minutoEHora} 28-31 * *";

                default:
                    throw new ArgumentException($"Tipo de recorrência inválido: {agendamento.TipoRecorrencia}", nameof(agendamento));
            }
        }

        // Primeiro dia, estritamente apos 'apartirDe', cujo DayOfWeek esteja em diasSemana --
        // sempre acha em ate 7 dias porque diasSemana cobre pelo menos 1 dos 7 possiveis.
        private static DateTime ProximoDiaDaSemana(TimeSpan horaDoDisparo, DateTime apartirDe, List<int> diasSemana)
        {
            for (var i = 1; i <= 7; i++)
            {
                var candidato = apartirDe.AddDays(i);
                if (diasSemana.Contains((int)candidato.DayOfWeek))
                    return candidato.Add(horaDoDisparo);
            }

            // Nunca deveria cair aqui (diasSemana valido sempre acha algo em 7 dias).
            return apartirDe.AddDays(7).Add(horaDoDisparo);
        }
    }
}
