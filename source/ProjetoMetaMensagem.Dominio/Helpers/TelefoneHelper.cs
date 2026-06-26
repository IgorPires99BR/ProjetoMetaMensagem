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
    }
}
