using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaDisparosLote
{
    // Com LoteId: aquele lote, com quem precisa de atencao. Sem: os mais recentes da empresa.
    public class ListaDisparosLoteCommand : IRequest<Response<ListaDisparosLoteResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid? LoteId { get; set; }

        // Vindo do token; nulo so para admin da plataforma.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
