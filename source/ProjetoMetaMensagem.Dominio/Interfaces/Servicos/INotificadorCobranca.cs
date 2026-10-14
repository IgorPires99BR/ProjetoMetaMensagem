using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Avisa a tela de Cobrancas de Clientes, em tempo real, que uma cobranca mudou (paga pelo
    // webhook do Asaas ou pela consulta periodica) -- sem isso a empresa so via o pagamento
    // depois de clicar em Atualizar.
    //
    // Mesmo esquema do INotificadorChat: interface no dominio, implementacao no WebAPI (SignalR).
    public interface INotificadorCobranca
    {
        // Nunca lanca excecao: avisar a tela e acessorio, a baixa ja foi gravada.
        Task NotificarCobrancaAtualizadaAsync(Guid empresaId, Guid cobrancaId, string status);
    }
}
