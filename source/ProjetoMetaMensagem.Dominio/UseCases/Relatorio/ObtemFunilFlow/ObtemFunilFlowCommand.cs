using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemFunilFlow
{
    // Responde a pergunta que faltava desde que a campanha comecou: em qual etapa do flow as
    // conversas param. Ate agora so dava pra saber QUANTAS pessoas responderam (relatorio de
    // engajamento), nunca ONDE elas desistiram.
    public class ObtemFunilFlowCommand : IRequest<Response<ObtemFunilFlowResult>>
    {
        public Guid FlowId { get; set; }

        // Mesmo padrao de escopo do resto do dominio (ver ObtemEmpresaHandler /
        // ObtemRelatorioEngajamentoHandler): vem do token, nunca do corpo ou da query.
        public Guid? EmpresaIdSolicitante { get; set; }

        public bool SolicitanteEhAdmin { get; set; }
    }
}
