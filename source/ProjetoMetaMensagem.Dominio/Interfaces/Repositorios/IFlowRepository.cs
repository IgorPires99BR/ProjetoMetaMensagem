using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IFlowRepository
    {
        Task<FlowEtapa?> ObterEtapaPorId(Guid etapaId);
        Task<FlowEtapa?> ObterEtapaInicial(Guid flowId);
        Task<FlowEtapa?> ObterProximaEtapa(Guid etapaAtualId, string respostaCliente);
        Task<Flow?> ObterPorId(Guid id);
        Task<IEnumerable<Flow>> ObterTodosPorEmpresa(Guid empresaId);

        // Métodos de escrita (Persistência)
        Task Incluir(Flow flow);
        Task IncluirEtapa(FlowEtapa etapa);

        // empresaIdSolicitante restringe a operacao aos fluxos da empresa informada.
        // null = administrador (sem restricao). Flow tem EmpresaId proprio; FlowEtapa nao,
        // e chega na empresa pelo Flow.
        Task<int> ExcluirEtapasPorFlowId(Guid flowId, Guid? empresaIdSolicitante);
        Task<int> Alterar(Flow flow, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
    }
}
