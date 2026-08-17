using Newtonsoft.Json.Linq;
using System;

namespace ProjetoMetaMensagem.Dominio.Help.Error
{
    // Erro devolvido pela propria Meta (Graph API), separado das exceções técnicas do sistema.
    // Sem esta distinção, a recusa da Meta ("template em análise não pode ser editado", por
    // exemplo) virava o genérico "Não foi possível concluir a operação" e o usuário não tinha
    // como saber o que corrigir.
    public class MetaApiException : Exception
    {
        public MetaApiException(string mensagemParaUsuario, string corpoOriginal)
            : base(mensagemParaUsuario)
        {
            CorpoOriginal = corpoOriginal;
        }

        // Resposta crua da Meta, só para o log do servidor
        public string CorpoOriginal { get; }

        // A Meta devolve o erro em `error`, com três textos de qualidade decrescente:
        // error_user_msg (escrito para o usuário final), error_user_title e message (técnico).
        public static MetaApiException DoCorpo(string corpoResposta, string acao)
        {
            var mensagem = ExtrairMensagem(corpoResposta);

            return new MetaApiException(
                string.IsNullOrWhiteSpace(mensagem) ? $"A Meta recusou {acao}." : $"A Meta recusou {acao}: {mensagem}",
                corpoResposta);
        }

        private static string? ExtrairMensagem(string corpoResposta)
        {
            if (string.IsNullOrWhiteSpace(corpoResposta) || !corpoResposta.TrimStart().StartsWith("{"))
                return null;

            try
            {
                var erro = JObject.Parse(corpoResposta)["error"];
                if (erro == null)
                    return null;

                var userMsg = erro["error_user_msg"]?.ToString();
                if (!string.IsNullOrWhiteSpace(userMsg))
                    return userMsg;

                var userTitle = erro["error_user_title"]?.ToString();
                if (!string.IsNullOrWhiteSpace(userTitle))
                    return userTitle;

                return erro["message"]?.ToString();
            }
            catch
            {
                // Corpo fora do formato esperado: melhor não ter mensagem do que ter lixo
                return null;
            }
        }
    }
}
