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

            // Endpoints que recebem "usuarioId" (ex: /contato/obter-por-usuario/{usuarioId},
            // /numero/ListarNumeros/{usuarioId}) escapavam da checagem por empresa, entao dava
            // pra ler contatos e numeros de outro tenant passando o id de um usuario de la.
            // Todos os chamadores do frontend mandam o id do proprio usuario logado, entao
            // exigir isso de quem nao e admin nao muda nenhum fluxo legitimo.
            var usuarioIdToken = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(usuarioIdToken)
                && context.ActionArguments.TryGetValue("usuarioId", out var usuarioIdRota)
                && usuarioIdRota != null
                && !string.Equals(usuarioIdRota.ToString(), usuarioIdToken, StringComparison.OrdinalIgnoreCase))
            {
                Negar(context);
                return;
            }

            // Nos controllers da propria Empresa, um parametro chamado so "id" (rota) ou uma
            // propriedade "Id" (corpo) É o id da empresa -- ex: DELETE /v2/empresa/excluir/{id}
            // e PUT /v2/empresa/alterar { id }. Fora desse controller, "Id" significa outra
            // coisa (id de contato, de template...), entao so vale aqui.
            var ehControllerDeEmpresa = context.Controller?.GetType().Name
                .StartsWith("Empresas", StringComparison.OrdinalIgnoreCase) == true;

            bool EhNomeDeEmpresa(string nome) =>
                NomesParametroEmpresa.Contains(nome, StringComparer.OrdinalIgnoreCase)
                || (ehControllerDeEmpresa && string.Equals(nome, "id", StringComparison.OrdinalIgnoreCase));

            foreach (var argumento in context.ActionArguments)
            {
                if (argumento.Value == null) continue;

                // Caso 1: o id da empresa veio solto na rota ou na query string.
                if (EhNomeDeEmpresa(argumento.Key))
                {
                    if (!Confere(argumento.Value.ToString(), empresaIdToken))
                    {
                        Negar(context);
                        return;
                    }
                    continue;
                }

                // Caso 2: o id veio dentro do corpo da requisicao (ex: [FromBody] AlteraEmpresaCommand).
                // Antes so o caso 1 era checado, entao bastava mandar o id no corpo pra escrever na
                // empresa de outro tenant -- dava pra renomear a empresa alheia e sobrescrever o
                // MetaAccessToken dela usando um usuario comum de outra empresa.
                var tipo = argumento.Value.GetType();
                if (tipo.IsPrimitive || tipo == typeof(string) || tipo == typeof(Guid)) continue;

                foreach (var prop in tipo.GetProperties())
                {
                    if (!EhNomeDeEmpresa(prop.Name)) continue;
                    if (!prop.CanRead) continue;

                    var valorProp = prop.GetValue(argumento.Value)?.ToString();
                    if (string.IsNullOrEmpty(valorProp)) continue;

                    if (!Confere(valorProp, empresaIdToken))
                    {
                        Negar(context);
                        return;
                    }
                }
            }
        }

        private static bool Confere(string? valor, string empresaIdToken)
            => string.Equals(valor, empresaIdToken, StringComparison.OrdinalIgnoreCase);

        private static void Negar(ActionExecutingContext context)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(
                new { erro = "Acesso negado: você não tem permissão para acessar dados de outra empresa." })
            {
                StatusCode = 403
            };
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
