using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Altera
{
    public class AlteraPipelineCommand : IRequest<Response<AlteraPipelineResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class AlteraPipelineResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
