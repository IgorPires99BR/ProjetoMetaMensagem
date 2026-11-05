using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.AlteraAtivoCobrancaCliente
{
    public class AlteraAtivoCobrancaClienteResult
    {
        public Guid CobrancaClienteId { get; set; }
        public bool Ativo { get; set; }
    }
}
