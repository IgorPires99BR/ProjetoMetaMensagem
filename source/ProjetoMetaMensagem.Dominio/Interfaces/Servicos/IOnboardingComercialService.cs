using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Pós-venda dentro da própria plataforma: quem acabou de comprar vira contato na conta de
    // operação da Contact Solution e recebe a mensagem de boas-vindas no WhatsApp.
    //
    // Nada aqui pode derrubar a criação da conta: a venda já aconteceu, e falha de Meta ou de
    // rede não pode desfazer um cliente pagante.
    public interface IOnboardingComercialService
    {
        // avisarPagamentoConfirmado=false para conta cadastrada pela equipe antes do pagamento:
        // avisar de um pagamento que nao aconteceu confunde quem recebe.
        Task ReceberNovoClienteAsync(string nomeComprador, string? telefoneComprador, string emailComprador, Guid empresaDoCliente, string? plano = null, bool avisarPagamentoConfirmado = true);
    }
}
