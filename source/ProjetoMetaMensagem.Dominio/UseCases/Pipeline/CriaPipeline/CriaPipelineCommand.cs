using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.CriaPipeline
{
    public class CriaPipelineCommand : IRequest<Response<CriaPipelineResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
