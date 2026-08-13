using Microsoft.Extensions.Configuration;

namespace ProjetoMetaMensagem.Servico.Auth
{
    // Existem DOIS niveis de administracao, e confundi-los vaza dado entre empresas:
    //
    // - Usuario.IsAdmin (claim "isAdmin"): admin da PROPRIA empresa. Manda em tudo dentro
    //   dela (numeros, usuarios, templates, credenciais da Meta) e nao enxerga as outras.
    // - Admin de plataforma (claim "isAdminPlataforma"): as contas de operacao da propria
    //   Contact Solution, que precisam de acesso cruzado pra configurar, testar e dar
    //   suporte. So estas dispensam o recorte por empresa.
    //
    // A lista fica no codigo de proposito: appsettings.json e appsettings.Development.json
    // sao gitignored neste repo, entao no arquivo ela ficaria invisivel na revisao e
    // dependeria de env var no Render pra existir em producao. Aqui ela aparece no diff,
    // e promover alguem exige commit + deploy, que e a barreira que se espera pra isso.
    // Ainda da pra sobrescrever via PlataformaAdmins:Emails sem mexer no codigo.
    public static class AdminsDaPlataforma
    {
        private static readonly string[] Padrao =
        {
            "vtrz@gmail.com",
            "igorpires97@gmail.com"
        };

        public static bool Contem(IConfiguration configuration, string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            // GetChildren em vez de Get<string[]>() pra nao depender do pacote
            // Configuration.Binder, que este projeto nao referencia.
            var configurados = configuration.GetSection("PlataformaAdmins:Emails")
                .GetChildren()
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();

            var lista = configurados.Length > 0 ? configurados : Padrao;

            return lista.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
