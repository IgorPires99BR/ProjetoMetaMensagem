using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface ICriacaoDeContaDeCliente
    {
        Task<ContaDeClienteCriada> CriarAsync(DadosDaContaDeCliente dados);
    }

    public class DadosDaContaDeCliente
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Cnpj { get; set; }
        public string Plano { get; set; } = string.Empty;

        // Decide se o cliente recebe o WhatsApp de "pagamento confirmado, um atendente vai
        // ligar". Verdadeiro quando a conta nasce de uma compra; falso quando a equipe cadastra
        // o cliente antes de ele pagar -- avisar de um pagamento que nao aconteceu confunde
        // quem recebe e queima a confianca logo no primeiro contato.
        public bool PagamentoJaConfirmado { get; set; }
    }

    public class ContaDeClienteCriada
    {
        public Guid EmpresaId { get; set; }

        // Devolvida para quem cadastrou poder passar o acesso na hora, por telefone ou WhatsApp,
        // sem depender do e-mail ter chegado.
        public string SenhaProvisoria { get; set; } = string.Empty;
    }
}
