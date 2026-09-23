using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga
{
    // Marcacao manual de pagamento -- por enquanto e o unico jeito confiavel de confirmar uma
    // CobrancaCliente: o link de pagamento ainda e o mesmo checkout compartilhado da Contact
    // Solution, entao nao da pra casar automaticamente qual pagamento e de qual cobranca via
    // webhook da Cakto (ver comentario em CobrancaCliente.cs / BD/48).
    public class MarcaCobrancaClientePagaCommand : IRequest<Response<MarcaCobrancaClientePagaResult>>
    {
        public Guid CobrancaClienteId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota).
        public Guid? EmpresaIdSolicitante { get; set; }

        // Nula = agora. Permite registrar um pagamento que ja aconteceu (ex: o Gilson confere o
        // extrato do dia anterior e marca com a data real do Pix, nao a data em que ele lembrou
        // de marcar).
        public DateTime? DataPagamento { get; set; }
    }
}
