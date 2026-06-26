using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IDashboardRepository
    {
        Task<ObterMetricasDashboardResult> ObterMetricas(Guid empresaId);
    }
}
