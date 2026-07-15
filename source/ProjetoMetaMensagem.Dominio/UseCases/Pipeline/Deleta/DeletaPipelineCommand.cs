using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Deleta
{
    public class DeletaPipelineCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public DeletaPipelineCommand(Guid id) => Id = id;
    }
}
