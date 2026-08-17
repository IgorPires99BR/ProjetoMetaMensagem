using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter
{
    // Monta o texto que o cliente realmente recebeu, trocando {{1}}, {{2}}... pelos valores
    // enviados. Antes disso, o HistoricoDisparo guardava só o JSON do payload, e o chat e o
    // relatório exibiam esse JSON cru pro usuário -- ninguém conseguia saber o que foi enviado.
    public static class TemplateTextoHelper
    {
        private static readonly Regex Variavel = new(@"\{\{(\d+)\}\}", RegexOptions.Compiled);

        public static string MontarTextoEnviado(string? conteudoTemplate, string? nomeTemplate, IList<string>? parametrosBody)
        {
            if (string.IsNullOrWhiteSpace(conteudoTemplate))
            {
                // Sem o texto do template em mãos (template apagado, por exemplo), pelo menos o
                // nome e os valores enviados são legíveis.
                var valores = parametrosBody != null && parametrosBody.Count > 0
                    ? $" ({string.Join(", ", parametrosBody)})"
                    : string.Empty;

                return $"📄 Modelo: {nomeTemplate}{valores}".Trim();
            }

            return Aplicar(conteudoTemplate!, parametrosBody);
        }

        public static string Aplicar(string conteudoTemplate, IList<string>? parametrosBody)
        {
            if (parametrosBody == null || parametrosBody.Count == 0)
                return conteudoTemplate;

            return Variavel.Replace(conteudoTemplate, match =>
            {
                // {{1}} é o primeiro parâmetro: a Meta numera a partir de 1, a lista a partir de 0.
                if (!int.TryParse(match.Groups[1].Value, out var indice) || indice < 1 || indice > parametrosBody.Count)
                    return match.Value;

                return parametrosBody[indice - 1];
            });
        }
    }
}
