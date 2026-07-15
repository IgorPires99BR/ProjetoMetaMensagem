using Dapper;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DbSession _session;

        public DashboardRepository(DbSession session)
            => _session = session;

        public async Task<ObterMetricasDashboardResult> ObterMetricas(Guid empresaId)
        {
            var result = new ObterMetricasDashboardResult();

            return result;
        }
    }
}

