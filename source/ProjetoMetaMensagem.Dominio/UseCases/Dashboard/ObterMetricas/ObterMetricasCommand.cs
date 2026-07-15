using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas
{
    public class ObterMetricasCommand : IRequest<Response<ObterMetricasDashboardResult>>
    {
        public Guid EmpresaId { get; set; }
        public ObterMetricasCommand(Guid empresaId) => EmpresaId = empresaId;
    }
}
