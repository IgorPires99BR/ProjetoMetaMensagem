namespace ProjetoMetaMensagem.Servico.Meta
{
    public static class TelefoneHelper
    {
        public static string FormatarParaMeta(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return telefone;

            // Remove tudo que não é dígito
            var digitos = new string(telefone.Where(char.IsDigit).ToArray());

            if (digitos.Length == 0)
                return telefone;

            // Se já começa com 55, só retorna os dígitos
            if (digitos.StartsWith("55"))
                return digitos;

            // Se tem 10 ou 11 dígitos (sem código de país), adiciona 55
            if (digitos.Length == 10 || digitos.Length == 11)
                return $"55{digitos}";

            // Qualquer outro caso, retorna só os dígitos
            return digitos;
        }
    }
}
