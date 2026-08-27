using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente
{
    // Cadastro interno: quem cria a conta do cliente e a equipe da Contact Solution, nao o
    // cliente. O cliente so recebe o acesso pronto.
    public class CriaContaClienteCommand : IRequest<Response<CriaContaClienteResult>>
    {
        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        public string? Cnpj { get; set; }

        public string Plano { get; set; } = string.Empty;

        // Preenchido para dar acesso a uma empresa que JA existe e nunca teve usuario. Vazio
        // cria empresa nova. Serve para empresas cadastradas antes de este fluxo existir, que
        // sem usuario sao peso morto: ninguem entra nelas.
        public Guid? EmpresaId { get; set; }

        // Marcar quando o cliente ja pagou por fora (PIX, transferencia, venda no balcao):
        // so entao ele recebe o WhatsApp dizendo que um atendente vai ligar.
        public bool PagamentoJaConfirmado { get; set; }

        // Vem do token, nunca do corpo: criar conta de cliente e privilegio da conta de
        // operacao da Contact Solution, nao de qualquer admin de qualquer empresa.
        public bool SolicitanteEhAdminDaPlataforma { get; set; }
    }
}
