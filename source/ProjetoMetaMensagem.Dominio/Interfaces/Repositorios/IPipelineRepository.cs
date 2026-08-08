using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IPipelineRepository
    {
        Task Incluir(Pipeline pipeline);
        // empresaIdSolicitante restringe a operacao ao funil da empresa informada.
        // null = administrador (sem restricao). Pipeline e LeadPipeline tem EmpresaId proprio;
        // PipelineEtapa nao, e chega na empresa pelo Pipeline.
        Task<int> Alterar(Pipeline pipeline, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Pipeline?> ObterPorId(Guid id);
        Task<List<Pipeline>> ListarPorEmpresa(Guid empresaId);
        Task<List<PipelineEtapa>> ListarEtapas(Guid pipelineId);
        Task IncluirEtapa(PipelineEtapa etapa);
        Task<int> AlterarEtapa(PipelineEtapa etapa, Guid? empresaIdSolicitante);
        Task<int> ExcluirEtapa(Guid id, Guid? empresaIdSolicitante);
        Task<PipelineEtapa?> ObterEtapaPorId(Guid id);
        Task<List<LeadPipeline>> ListarLeads(Guid empresaId);
        Task IncluirLead(LeadPipeline lead);
        Task<int> MoverLead(Guid leadId, Guid novaEtapaId, Guid? empresaIdSolicitante);
        Task<int> RemoverLead(Guid leadId, Guid? empresaIdSolicitante);
        Task<LeadPipeline?> ObterLead(Guid id);
        Task<bool> LeadJaExiste(Guid empresaId, Guid contatoId);
        Task<int> ContarLeadsPorEtapa(Guid etapaId, Guid? empresaIdSolicitante);
    }
}
