using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas
{
    public class ObterMetricasHandler : IRequestHandler<ObterMetricasCommand, Response<ObterMetricasDashboardResult>>
    {
        private readonly IDashboardRepository _dashboardRepository;

        private readonly ILogger<ObterMetricasHandler> _logger;

        public ObterMetricasHandler(IDashboardRepository dashboardRepository, ILogger<ObterMetricasHandler> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

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
                response.AddErroServico(ex, _logger, nameof(ObterMetricasHandler));
            }
            return response;
        }
    }
}
