using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    // O dashboard do HangFire (/hangfire) e uma pagina HTML acessada direto no navegador, fora
    // do fluxo de login da SPA -- nao da pra anexar o JWT numa navegacao comum. Basic Auth
    // dedicado (usuario/senha proprios, nada a ver com login de cliente) e o jeito simples e
    // seguro de nao deixar isso publico: o navegador mostra o prompt nativo de credencial.
    public class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
    {
        private readonly string _usuario;
        private readonly string _senha;

        public HangfireBasicAuthFilter(IConfiguration configuration)
        {
            _usuario = configuration["Hangfire:DashboardUsuario"] ?? "admin";
            _senha = configuration["Hangfire:DashboardSenha"]
                ?? throw new InvalidOperationException("Hangfire:DashboardSenha nao configurado. Defina essa configuracao (appsettings ou variavel de ambiente) antes de iniciar a API.");
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var header = httpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                PedirCredencial(httpContext);
                return false;
            }

            try
            {
                var credenciais = Encoding.UTF8.GetString(Convert.FromBase64String(header.Substring("Basic ".Length)));
                var partes = credenciais.Split(':', 2);
                if (partes.Length == 2 && partes[0] == _usuario && partes[1] == _senha)
                    return true;
            }
            catch (FormatException)
            {
                // header Authorization mal formado -- cai no PedirCredencial abaixo.
            }

            PedirCredencial(httpContext);
            return false;
        }

        private static void PedirCredencial(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"HangFire Dashboard\"";
            httpContext.Response.StatusCode = 401;
        }
    }
}
