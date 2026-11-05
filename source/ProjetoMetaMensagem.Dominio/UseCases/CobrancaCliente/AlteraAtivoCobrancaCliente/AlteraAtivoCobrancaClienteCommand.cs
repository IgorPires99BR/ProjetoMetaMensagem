using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.AlteraAtivoCobrancaCliente
{
    // Tira da tela (ou traz de volta) uma cobranca antiga ou acertada por fora, sem apagar nem
    // mudar o Status dela.
    public class AlteraAtivoCobrancaClienteCommand : IRequest<Response<AlteraAtivoCobrancaClienteResult>>
    {
        public Guid CobrancaClienteId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota).
        public Guid? EmpresaIdSolicitante { get; set; }

        public bool Ativo { get; set; }
    }
}
