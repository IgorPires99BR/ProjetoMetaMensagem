namespace ProjetoMetaMensagem.Dominio.Helpers
{
    public static class TelefoneHelper
    {
        public static string FormatarParaMeta(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return telefone;

            var digitos = new string(telefone.Where(char.IsDigit).ToArray());

            if (digitos.Length == 0)
                return telefone;

            if (digitos.StartsWith("55"))
                return digitos;

            if (digitos.Length == 10 || digitos.Length == 11)
                return $"55{digitos}";

            return digitos;
        }

        // Formas de um mesmo numero para comparar com o cadastro. A Meta costuma mandar celular
        // brasileiro antigo sem o 9 (5511 8888-8888) enquanto o cadastro tem com ele; sem isto a
        // resposta do cliente nao casava com o contato, que era criado de novo.
        public static IReadOnlyList<string> VariantesParaBusca(string telefone)
        {
            var digitos = new string((telefone ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digitos.Length == 0) return Array.Empty<string>();

            var nacional = digitos.StartsWith("55") && digitos.Length >= 12 ? digitos.Substring(2) : digitos;
            if (nacional.Length != 10 && nacional.Length != 11) return new[] { digitos };

            var ddd = nacional.Substring(0, 2);
            var numero = nacional.Substring(2);
            var formas = new List<string> { nacional };

            if (numero.Length == 9 && numero[0] == '9')
                formas.Add(ddd + numero.Substring(1));
            // So celular (6 a 9) ganha o 9: em telefone fixo ele formaria outro numero.
            else if (numero.Length == 8 && numero[0] >= '6')
                formas.Add(ddd + "9" + numero);

            return formas.SelectMany(f => new[] { f, "55" + f }).Append(digitos).Distinct().ToList();
        }
    }
}
