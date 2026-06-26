using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas
{
    public class ObterMetricasHandler : IRequestHandler<ObterMetricasCommand, Response<ObterMetricasDashboardResult>>
    {
        private readonly IDashboardRepository _dashboardRepository;

        public ObterMetricasHandler(IDashboardRepository dashboardRepository)
            => _dashboardRepository = dashboardRepository;

        public async Task<Response<ObterMetricasDashboardResult>> Handle(ObterMetricasCommand command)
        {
            var response = new Response<ObterMetricasDashboardResult>();
            try
            {
                var metricas = await _dashboardRepository.ObterMetricas(command.EmpresaId);
                response.AddValue(metricas);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao obter métricas: {ex.Message}");
            }
            return response;
        }
    }
}
