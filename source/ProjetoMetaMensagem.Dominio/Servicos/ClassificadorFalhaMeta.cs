using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    public static class ClassificadorFalhaMeta
    {
        // Limites de envio. Vem com HTTP 400 em varios casos, por isso o codigo pesa mais que o status.
        private static readonly HashSet<long> CodigosDeLimite = new() { 4, 80007, 130429, 131056 };

        // 2 = servico temporariamente indisponivel, recusado antes de processar.
        private const long CodigoIndisponivel = 2;

        // 1 e 131000 = erro desconhecido do lado da Meta: nao da para afirmar que nao saiu.
        private static readonly HashSet<long> CodigosDesconhecidos = new() { 1, 131000 };

        public static TipoFalhaEnvio DaResposta(int statusHttp, string? corpo)
        {
            var codigo = CodigoDoErro(corpo);
            if (codigo.HasValue)
            {
                if (CodigosDeLimite.Contains(codigo.Value) || codigo == CodigoIndisponivel) return TipoFalhaEnvio.Transitoria;
                if (CodigosDesconhecidos.Contains(codigo.Value)) return TipoFalhaEnvio.Incerta;
            }

            if (statusHttp == 429 || statusHttp == 503) return TipoFalhaEnvio.Transitoria;
            return statusHttp >= 500 ? TipoFalhaEnvio.Incerta : TipoFalhaEnvio.Definitiva;
        }

        // So recusa de conexao e DNS garantem que o pedido nao chegou. Timeout e conexao caida no
        // meio nao garantem nada.
        public static TipoFalhaEnvio DaExcecao(Exception ex)
        {
            var socket = Cadeia(ex).OfType<SocketException>().FirstOrDefault();
            if (ex is HttpRequestException && socket != null && socket.SocketErrorCode is
                    SocketError.ConnectionRefused or SocketError.HostNotFound or SocketError.TryAgain
                    or SocketError.NetworkUnreachable or SocketError.HostUnreachable)
            {
                return TipoFalhaEnvio.Transitoria;
            }

            return TipoFalhaEnvio.Incerta;
        }

        private static IEnumerable<Exception> Cadeia(Exception ex)
        {
            for (var atual = ex; atual != null; atual = atual.InnerException)
                yield return atual;
        }

        private static long? CodigoDoErro(string? corpo)
        {
            if (string.IsNullOrWhiteSpace(corpo) || !corpo.TrimStart().StartsWith("{")) return null;

            try
            {
                return JObject.Parse(corpo)["error"]?["code"]?.Value<long?>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
