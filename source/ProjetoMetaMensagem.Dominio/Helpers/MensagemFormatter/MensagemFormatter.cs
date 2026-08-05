using Newtonsoft.Json.Linq;
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

                if (jsonObj["PayloadEnvio"] != null)
                {
                    var payloadObj = JObject.Parse(jsonObj["PayloadEnvio"]!.ToString());
                    var templateName = payloadObj["template"]?["name"]?.ToString();

                    if (!string.IsNullOrEmpty(templateName))
                        return $"📄 [Template: {templateName}]";
                }
            }
            catch
            {
                // Nao era o JSON esperado de template: cai no fallback abaixo
            }

            return _htmlHelper.LimparHtml(conteudo);
        }
    }
}
