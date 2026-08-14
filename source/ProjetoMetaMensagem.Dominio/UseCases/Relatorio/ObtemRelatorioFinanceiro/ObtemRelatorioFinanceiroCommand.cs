using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro
{
    public class ObtemRelatorioFinanceiroCommand : IRequest<Response<ObtemRelatorioFinanceiroResult>>
    {
        // Preenchidos pelo controller a partir das claims do JWT, nunca vindos do cliente
        // (mesmo padrao de ObtemEmpresaCommand) -- e um relatorio cross-tenant, so admin ve.
        public bool SolicitanteEhAdmin { get; set; }

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
