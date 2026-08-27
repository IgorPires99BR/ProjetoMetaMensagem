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
        // Inclui sempre os flows genericos (NumeroId IS NULL) alem dos especificos do numero
        // informado. numeroId null retorna so os genericos.
        Task<IEnumerable<Flow>> ObterTodosPorEmpresaENumero(Guid empresaId, Guid? numeroId);

        // Métodos de escrita (Persistência)
        Task Incluir(Flow flow);
        Task IncluirEtapa(FlowEtapa etapa);

        // empresaIdSolicitante restringe a operacao aos fluxos da empresa informada.
        // null = administrador (sem restricao). Flow tem EmpresaId proprio; FlowEtapa nao,
        // e chega na empresa pelo Flow.
        Task<int> ExcluirEtapasPorFlowId(Guid flowId, Guid? empresaIdSolicitante);

        // Atualiza uma etapa mantendo o Id. E o que permite editar um flow sem recriar tudo:
        // conversa em andamento guarda o Id da etapa atual, e recriar a etapa com Id novo
        // deixava esse ponteiro apontando pra algo que nao existe mais.
        Task<int> AlterarEtapa(FlowEtapa etapa);

        // Etapas de um flow, sem carregar o flow inteiro. Usado na edicao pra saber o que ja
        // existe e decidir entre atualizar, incluir ou excluir cada etapa.
        Task<List<FlowEtapa>> ObterEtapasPorFlow(Guid flowId);

        // Exclui uma etapa especifica (a que o usuario removeu do flow na edicao).
        Task<int> ExcluirEtapa(Guid etapaId);
        Task<int> Alterar(Flow flow, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
    }
}
