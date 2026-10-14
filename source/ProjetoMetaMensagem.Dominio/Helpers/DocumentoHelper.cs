using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.Helpers
{
    // CPF/CNPJ do pagador e da subconta Asaas: o Asaas recusa documento com digito verificador
    // errado, e a recusa so apareceria no disparo, contato por contato. Confere antes.
    public static class DocumentoHelper
    {
        // Null para vazio; senao so os digitos.
        public static string? SoDigitos(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            var digitos = new string(valor.Where(char.IsDigit).ToArray());
            return digitos.Length == 0 ? null : digitos;
        }

        public static bool EhCpf(string? digitos) => digitos?.Length == 11;
        public static bool EhCnpj(string? digitos) => digitos?.Length == 14;

        public static bool CpfCnpjValido(string? valor)
        {
            var d = SoDigitos(valor);
            if (d == null || (d.Length != 11 && d.Length != 14)) return false;
            // 000.000.000-00, 111.111.111-11... passam no calculo mas nao existem.
            if (d.Distinct().Count() == 1) return false;

            return d.Length == 11
                ? DigitoConfere(d, 9, new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 }) && DigitoConfere(d, 10, new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 })
                : DigitoConfere(d, 12, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }) && DigitoConfere(d, 13, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        }

        private static bool DigitoConfere(string digitos, int posicao, int[] pesos)
        {
            var soma = 0;
            for (var i = 0; i < pesos.Length; i++)
                soma += (digitos[i] - '0') * pesos[i];

            var resto = soma % 11;
            var esperado = resto < 2 ? 0 : 11 - resto;
            return digitos[posicao] - '0' == esperado;
        }
    }
}
