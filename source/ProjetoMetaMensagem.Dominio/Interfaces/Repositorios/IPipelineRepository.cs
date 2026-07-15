using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IPipelineRepository
    {
        Task Incluir(Pipeline pipeline);
        Task Alterar(Pipeline pipeline);
        Task Excluir(Guid id);
        Task<Pipeline?> ObterPorId(Guid id);
        Task<List<Pipeline>> ListarPorEmpresa(Guid empresaId);
        Task<List<PipelineEtapa>> ListarEtapas(Guid pipelineId);
        Task IncluirEtapa(PipelineEtapa etapa);
        Task AlterarEtapa(PipelineEtapa etapa);
        Task ExcluirEtapa(Guid id);
        Task<PipelineEtapa?> ObterEtapaPorId(Guid id);
        Task<List<LeadPipeline>> ListarLeads(Guid empresaId);
        Task IncluirLead(LeadPipeline lead);
        Task MoverLead(Guid leadId, Guid novaEtapaId);
        Task RemoverLead(Guid leadId);
        Task<LeadPipeline?> ObterLead(Guid id);
        Task<bool> LeadJaExiste(Guid empresaId, Guid contatoId);
        Task<int> ContarLeadsPorEtapa(Guid etapaId);
    }
}
