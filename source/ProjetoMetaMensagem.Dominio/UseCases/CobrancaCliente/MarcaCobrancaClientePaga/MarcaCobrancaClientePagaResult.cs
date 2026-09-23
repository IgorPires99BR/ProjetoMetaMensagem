using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga
{
    public class MarcaCobrancaClientePagaResult
    {
        public Guid CobrancaClienteId { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
