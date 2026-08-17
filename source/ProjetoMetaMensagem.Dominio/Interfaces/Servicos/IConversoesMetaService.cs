using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Reporta a venda de volta para a Meta (Conversions API), fechando o ciclo do anúncio.
    //
    // Sem isto o algoritmo só sabe quem iniciou conversa, nunca quem pagou -- e passa a vida
    // otimizando pelo sinal errado. Dois caminhos, conforme a origem do cliente:
    //   - veio de anúncio Click-to-WhatsApp: usa o ctwa_clid gravado na primeira mensagem
    //   - veio da landing/checkout: usa fbc/fbp gravados na assinatura
    public interface IConversoesMetaService
    {
        Task<bool> ReportarCompraAsync(
            string emailComprador,
            string? telefoneComprador,
            decimal valor,
            string? idPedido,
            string? ctwaClid,
            string? fbc,
            string? fbp);
    }
}
