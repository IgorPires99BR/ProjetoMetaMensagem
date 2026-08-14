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
        // Admin da propria empresa: manda em tudo dentro dela, mas NAO enxerga as outras.
        // Usado pra liberar telas de administracao, nunca pra dispensar recorte por empresa.
        public static bool EhAdmin(this ControllerBase controller) =>
            string.Equals(controller.User.FindFirst("isAdmin")?.Value, "true", StringComparison.OrdinalIgnoreCase);

        // Conta de operacao da propria Contact Solution (ver AdminsDaPlataforma no projeto
        // Servico): a unica que enxerga todas as empresas.
        public static bool EhAdminDaPlataforma(this ControllerBase controller) =>
            string.Equals(controller.User.FindFirst("isAdminPlataforma")?.Value, "true", StringComparison.OrdinalIgnoreCase);

        // Devolve null so para conta de plataforma, e os handlers tratam null como "nao filtrar
        // por empresa". Antes bastava ser admin: como IsAdmin e o admin do CLIENTE, isso dava a
        // ele leitura das outras empresas (contatos, por exemplo) assim que existisse mais de uma.
        public static Guid? EmpresaDoEscopo(this ControllerBase controller)
        {
            if (controller.EhAdminDaPlataforma()) return null;

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
