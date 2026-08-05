using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    // Garante isolamento multi-tenant: se a rota/query tiver um parametro de empresa
    // (idEmpresa, empresaId, idempresa, empresaid), ele precisa bater com a claim "empresaId"
    // do token JWT do usuario logado. Administradores (claim isAdmin=true) sao dispensados.
    public class EmpresaAccessFilter : IActionFilter
    {
        private static readonly string[] NomesParametroEmpresa = { "idEmpresa", "empresaId", "idempresa", "empresaid" };

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return; // endpoints [AllowAnonymous] nao tem usuario autenticado

            var isAdmin = user.FindFirst("isAdmin")?.Value;
            if (string.Equals(isAdmin, "true", StringComparison.OrdinalIgnoreCase))
                return;

            var empresaIdToken = user.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdToken))
                return;

            foreach (var nome in NomesParametroEmpresa)
            {
                if (context.ActionArguments.TryGetValue(nome, out var valor) && valor != null)
                {
                    var empresaIdRota = valor.ToString();
                    if (!string.Equals(empresaIdRota, empresaIdToken, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(
                            new { erro = "Acesso negado: você não tem permissão para acessar dados de outra empresa." })
                        {
                            StatusCode = 403
                        };
                        return;
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
