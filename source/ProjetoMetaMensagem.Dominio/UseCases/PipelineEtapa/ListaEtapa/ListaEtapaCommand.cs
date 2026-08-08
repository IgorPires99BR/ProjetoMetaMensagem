using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa
{
    public class ListaEtapaCommand : IRequest<Response<List<ListaEtapaResult>>>
    {
        public Guid PipelineId { get; set; }
        public ListaEtapaCommand(Guid pipelineId) => PipelineId = pipelineId;
    }
}
