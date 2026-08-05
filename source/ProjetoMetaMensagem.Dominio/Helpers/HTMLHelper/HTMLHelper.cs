using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ProjetoMetaMensagem.Dominio.Helpers.HTMLHelper
{
    public class HTMLHelper
    {
        public string LimparHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Decodifica caracteres especiais do HTML (ex: &amp; vira &, &lt; vira <)
            var textoSemHtml = HttpUtility.HtmlDecode(input);

            // 2. Remove todas as tags HTML/XML (tudo entre < e >)
            textoSemHtml = Regex.Replace(textoSemHtml, "<.*?>", string.Empty);

            // 3. Remove múltiplos espaços em branco / quebras de linha acumuladas
            return textoSemHtml.Trim();
        }
    }
}
