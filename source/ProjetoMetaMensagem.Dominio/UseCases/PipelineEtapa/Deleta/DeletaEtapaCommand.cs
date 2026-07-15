using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Deleta
{
    public class DeletaEtapaCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public DeletaEtapaCommand(Guid id) => Id = id;
    }
}
