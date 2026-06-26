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

        Task ExcluirEtapasPorFlowId(Guid flowId);
        Task Alterar(Flow flow);
        Task Excluir(Guid id);
    }
}
