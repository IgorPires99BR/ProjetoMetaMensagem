using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // O state do OAuth carrega a empresa de volta pelo navegador. Criptografado (AES-GCM, nonce
    // aleatorio): ninguem consegue forjar um state que ligue a conta dele a OUTRA empresa, e ele
    // expira -- um link de autorizacao esquecido aberto nao vale depois.
    public static class EstadoOAuthMercadoPago
    {
        private static readonly TimeSpan Validade = TimeSpan.FromMinutes(30);

        public static string Gerar(Guid empresaId, DateTime agora, ICriptografiaService criptografia) =>
            criptografia.Criptografar($"{empresaId:N}|{(agora + Validade).Ticks}");

        // Nulo = state invalido, adulterado ou vencido.
        public static Guid? Ler(string? state, DateTime agora, ICriptografiaService criptografia)
        {
            if (string.IsNullOrWhiteSpace(state)) return null;
            try
            {
                var partes = criptografia.Descriptografar(state).Split('|');
                if (partes.Length != 2 || !Guid.TryParseExact(partes[0], "N", out var empresaId) || !long.TryParse(partes[1], out var ticks))
                    return null;
                return new DateTime(ticks) >= agora ? empresaId : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
