using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ProjetoMetaMensagem.Dominio.Helpers.HTMLHelper;

namespace ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter
{
    // Converte o Conteudo bruto salvo em HistoricoDisparo (que pode ser um JSON de
    // template com ParametrosBody/ParametrosButton/PayloadEnvio) num texto legivel
    // pro chat. Usado tanto no historico paginado quanto no broadcast do SignalR,
    // pra nao existir dois lugares formatando a mensagem de jeitos diferentes.
    public static class MensagemFormatter
    {
        private static readonly HTMLHelper.HTMLHelper _htmlHelper = new();

        public static string FormatarConteudo(string? conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
                return string.Empty;

            if (!conteudo.TrimStart().StartsWith("{"))
                return _htmlHelper.LimparHtml(conteudo);

            try
            {
                var jsonObj = JObject.Parse(conteudo);

                var nomeTemplate = ExtrairNomeTemplate(jsonObj);
                var parametros = ExtrairParametrosBody(jsonObj);

                if (!string.IsNullOrEmpty(nomeTemplate) || parametros.Count > 0)
                {
                    var valores = parametros.Count > 0 ? $" ({string.Join(", ", parametros)})" : string.Empty;
                    var nome = string.IsNullOrEmpty(nomeTemplate) ? "modelo de mensagem" : nomeTemplate;

                    return $"📄 Modelo: {nome}{valores}";
                }
            }
            catch
            {
                // Nao era o JSON esperado de template: cai no fallback abaixo
            }

            // Ultimo recurso: nunca devolver o JSON cru pro usuario -- e o que fazia o chat
            // exibir {"ParametrosBody":[],"PayloadEnvio":... no lugar da mensagem.
            if (conteudo.TrimStart().StartsWith("{"))
                return "📄 Mensagem de modelo enviada";

            return _htmlHelper.LimparHtml(conteudo);
        }

        // O nome do template aparece em tres formatos diferentes de payload ja gravados:
        // PayloadEnvio.template.name, PayloadEnvio.name (lote) e NomeTemplate (flow).
        private static string? ExtrairNomeTemplate(JObject jsonObj)
        {
            var nomeDireto = jsonObj["NomeTemplate"]?.ToString();
            if (!string.IsNullOrEmpty(nomeDireto))
                return nomeDireto;

            var payload = jsonObj["PayloadEnvio"];
            if (payload == null)
                return null;

            try
            {
                var payloadObj = JObject.Parse(payload.ToString());
                return payloadObj["template"]?["name"]?.ToString() ?? payloadObj["name"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static List<string> ExtrairParametrosBody(JObject jsonObj)
        {
            var lista = new List<string>();

            if (jsonObj["ParametrosBody"] is JArray parametros)
            {
                foreach (var item in parametros)
                {
                    var valor = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(valor))
                        lista.Add(valor!);
                }
            }

            return lista;
        }
    }
}
