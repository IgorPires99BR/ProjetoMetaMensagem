using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Unica fonte de verdade de "de onde sai o valor de cada variavel do template", usada pelo
    // disparo em lote (tela de Disparos) e pelo processamento de Agendamentos -- antes cada um
    // tinha a sua copia da regra, e um campo novo (valorFatura) so existia num dos lados.
    public static class ResolvedorDeVariaveis
    {
        public const string Fixo = "fixo";
        public const string ParametroCadastrado = "parametro";

        // Campos do Contato que uma variavel (ou um Parametro CAMPO_CONTATO) pode puxar.
        public const string Nome = "nome";
        public const string NomeCliente = "nomeCliente";
        public const string Telefone = "telefone";
        public const string ValorFatura = "valorFatura";
        public const string DiaVencimento = "diaVencimento";
        // Data completa do vencimento no mes de referencia (ex: dia 20 em setembro -> 20/09/2026).
        public const string DataVencimento = "dataVencimento";
        public const string TaxaJuros = "taxaJuros";
        public const string TaxaJurosMensal = "taxaJurosMensal";

        public static readonly string[] CamposDoContato =
        {
            Nome, NomeCliente, Telefone, ValorFatura, DiaVencimento, DataVencimento, TaxaJuros, TaxaJurosMensal
        };

        // Sem casa decimal fixa em pt-BR (405 vira "405,00") o template sai com o valor cru do
        // banco (405.00, com ponto) -- errado pro publico brasileiro.
        private static readonly CultureInfo Brasil = new("pt-BR");

        public static bool OrigemValida(string? origem) =>
            origem == Fixo || origem == ParametroCadastrado || CamposDoContato.Contains(origem);

        // Diz se o valor muda de contato pra contato: so o texto fixo (direto ou via parametro
        // FIXO) vale igual pra todo mundo.
        public static bool DependeDoContato(AgendamentoVariavelDto variavel, IReadOnlyDictionary<Guid, Parametro> parametros)
        {
            if (variavel.Origem == Fixo) return false;

            if (variavel.Origem == ParametroCadastrado)
            {
                return variavel.ParametroId.HasValue
                    && parametros.TryGetValue(variavel.ParametroId.Value, out var parametro)
                    && parametro.Tipo == Parametro.CampoDoContato;
            }

            return true;
        }

        public static string Resolver(
            AgendamentoVariavelDto variavel, Contato? contato,
            IReadOnlyDictionary<Guid, Parametro> parametros, DateTime hoje)
        {
            if (variavel.Origem == Fixo)
                return (variavel.ValorFixo ?? string.Empty).Trim();

            if (variavel.Origem == ParametroCadastrado)
            {
                if (!variavel.ParametroId.HasValue || !parametros.TryGetValue(variavel.ParametroId.Value, out var parametro))
                    return string.Empty;

                return parametro.Tipo == Parametro.CampoDoContato
                    ? ResolverCampo(parametro.Valor, contato, hoje)
                    : (parametro.Valor ?? string.Empty).Trim();
            }

            return ResolverCampo(variavel.Origem, contato, hoje);
        }

        private static string ResolverCampo(string? campo, Contato? contato, DateTime hoje)
        {
            if (contato == null) return string.Empty;

            return campo switch
            {
                Nome => contato.NomeContato ?? string.Empty,
                NomeCliente => contato.NomeCliente ?? string.Empty,
                Telefone => contato.Telefone ?? string.Empty,
                ValorFatura => contato.ValorFatura?.ToString("N2", Brasil) ?? string.Empty,
                DiaVencimento => contato.DiaVencimento?.ToString() ?? string.Empty,
                DataVencimento => DataDeVencimento(contato.DiaVencimento, hoje),
                TaxaJuros => contato.TaxaJuros?.ToString("N2", Brasil) ?? string.Empty,
                TaxaJurosMensal => contato.TaxaJurosMensal?.ToString("N2", Brasil) ?? string.Empty,
                _ => string.Empty
            };
        }

        // Dia de vencimento do contato aplicado ao mes/ano de "hoje". Clampado pro ultimo dia do
        // mes quando o vencimento (ex: 31) nao existe no mes corrente -- mesmo criterio da
        // recorrencia Mensal e do VenceHoje do ProcessaAgendamentoHandler.
        public static string DataDeVencimento(int? diaVencimento, DateTime hoje)
        {
            if (!diaVencimento.HasValue) return string.Empty;

            var dia = Math.Min(diaVencimento.Value, DateTime.DaysInMonth(hoje.Year, hoje.Month));
            return new DateTime(hoje.Year, hoje.Month, dia).ToString("dd/MM/yyyy", Brasil);
        }

        // Preenche ParametrosBody (valores iguais pra todos) e ParametrosBodyPorTelefone (valores
        // de cada destinatario) do comando a partir das definicoes de variavel. Mesmo formato
        // que a tela de Disparos ja montava a mao: o slot "global" de uma variavel que depende do
        // contato fica vazio de proposito, o valor real vai por telefone.
        public static void Preencher(
            EnviarMensagemTemplateMetaLoteCommand comando,
            IReadOnlyList<AgendamentoVariavelDto> variaveis,
            IEnumerable<(string Telefone, Contato Contato)> destinatarios,
            IReadOnlyDictionary<Guid, Parametro> parametros,
            DateTime hoje)
        {
            if (variaveis == null || variaveis.Count == 0) return;

            comando.ParametrosBody = variaveis
                .Select(v => DependeDoContato(v, parametros) ? string.Empty : Resolver(v, null, parametros, hoje))
                .ToList();

            if (!variaveis.Any(v => DependeDoContato(v, parametros))) return;

            comando.ParametrosBodyPorTelefone = new Dictionary<string, List<string>>();
            foreach (var (telefone, contato) in destinatarios)
            {
                comando.ParametrosBodyPorTelefone[telefone] = variaveis
                    .Select(v => Resolver(v, contato, parametros, hoje))
                    .ToList();
            }
        }
    }
}
