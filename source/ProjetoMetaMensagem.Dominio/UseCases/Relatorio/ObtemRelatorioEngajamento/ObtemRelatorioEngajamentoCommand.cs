using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento
{
    public class ObtemRelatorioEngajamentoCommand : IRequest<Response<ObtemRelatorioEngajamentoResult>>
    {
        // Preenchidos pelo controller a partir das claims do JWT, nunca vindos do cliente.
        public bool SolicitanteEhAdmin { get; set; }
        public Guid? EmpresaIdSolicitante { get; set; }

        // So admin pode usar pra filtrar por um cliente especifico; se nulo, admin ve todos.
        public Guid? EmpresaIdFiltro { get; set; }

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
