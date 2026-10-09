using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Regras do Pix da cobranca ao cliente, decididas com o Igor (2026-10-05/06):
    // - Pix imediato (cob), com encargos calculados por nos. Nao usamos cobranca com vencimento
    //   (cobv), que exige CPF/CNPJ do pagador -- o Contato nao tem esse campo.
    // - TaxaJuros (%) e multa unica; TaxaJurosMensal (%) vira juros pro rata dia (mes de 30
    //   dias). Os dois so contam depois do vencimento.
    // - O Pix vale ate 23:59:59 do vencimento; se a cobranca ja sai vencida, ate o fim do dia
    //   do envio (no dia seguinte os juros mudam, entao o valor do Pix ficaria errado).
    public static class CalculoCobrancaPix
    {
        public record Resultado(decimal Valor, decimal Multa, decimal Juros, int DiasAtraso, DateTime ExpiraEm)
        {
            public decimal ValorCobrado => Valor + Multa + Juros;
        }

        public static Resultado Calcular(decimal valor, decimal? taxaMulta, decimal? taxaJurosMensal, DateTime vencimento, DateTime agora)
        {
            var diasAtraso = Math.Max(0, (agora.Date - vencimento.Date).Days);
            var vencido = diasAtraso > 0;

            var multa = vencido ? Arredondar(valor * (taxaMulta ?? 0m) / 100m) : 0m;
            var juros = vencido ? Arredondar(valor * (taxaJurosMensal ?? 0m) / 100m / 30m * diasAtraso) : 0m;

            var diaLimite = vencido ? agora.Date : vencimento.Date;
            var expiraEm = diaLimite.AddDays(1).AddSeconds(-1);

            return new Resultado(valor, multa, juros, diasAtraso, expiraEm);
        }

        // Mesmo calculo usado pela variavel valorAtualizado do template: o texto da mensagem e
        // o valor do Pix tem que bater centavo a centavo.
        public static decimal ValorAtualizado(decimal valor, decimal? taxaMulta, decimal? taxaJurosMensal, DateTime vencimento, DateTime agora) =>
            Calcular(valor, taxaMulta, taxaJurosMensal, vencimento, agora).ValorCobrado;

        private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    // O servidor de producao (Render) roda em UTC; vencimento e validade do Pix sao do dia do
    // cliente, no horario de Brasilia.
    public static class HorarioBrasilia
    {
        private static readonly TimeZoneInfo Fuso = ObterFuso();

        public static DateTime Agora() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso);

        public static DateTime ParaBrasilia(DateTimeOffset momento) => TimeZoneInfo.ConvertTime(momento, Fuso).DateTime;

        private static TimeZoneInfo ObterFuso()
        {
            foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            // Sem horario de verao desde 2019: UTC-3 fixo e equivalente.
            return TimeZoneInfo.CreateCustomTimeZone("Brasilia", TimeSpan.FromHours(-3), "Brasilia", "Brasilia");
        }
    }
}
