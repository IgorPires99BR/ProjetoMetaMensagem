using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    // O EmpresaAccessFilter compara ids de empresa que chegam na requisicao, mas nao consegue
    // saber a quem pertence uma entidade filha (um contato, um template, um flow) -- pra isso
    // seria preciso ir ao banco. Entao os comandos que operam sobre entidade filha precisam
    // carregar o escopo do usuario logado, e a consulta filtra por ele.
    //
    // O escopo SEMPRE vem das claims do JWT, nunca de algo que o cliente mandou: se viesse do
    // corpo ou da rota, o proprio atacante escolheria o escopo dele.
    public static class EscopoDoTokenExtensions
    {
        public static bool EhAdmin(this ControllerBase controller) =>
            string.Equals(controller.User.FindFirst("isAdmin")?.Value, "true", StringComparison.OrdinalIgnoreCase);

        // Devolve null para administradores, que por decisao de produto enxergam todas as empresas.
        // Os handlers tratam null como "nao filtrar por empresa".
        public static Guid? EmpresaDoEscopo(this ControllerBase controller)
        {
            if (controller.EhAdmin()) return null;

            var claim = controller.User.FindFirst("empresaId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty; // Guid.Empty nao casa com nada: nega por padrao
        }

        // Id do usuario (vendedor) logado, usado para registrar quem assumiu manualmente uma conversa.
        public static Guid? UsuarioIdDoEscopo(this ControllerBase controller)
        {
            var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
