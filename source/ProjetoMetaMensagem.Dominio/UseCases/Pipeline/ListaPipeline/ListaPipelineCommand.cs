using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline
{
    public class ListaPipelineCommand : IRequest<Response<List<ListaPipelineResult>>>
    {
        public Guid EmpresaId { get; set; }
        public ListaPipelineCommand(Guid empresaId) => EmpresaId = empresaId;
    }
}
