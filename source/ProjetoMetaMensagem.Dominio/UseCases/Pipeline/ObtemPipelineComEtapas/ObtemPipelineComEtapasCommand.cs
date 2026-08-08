using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas
{
    public class ObtemPipelineComEtapasCommand : IRequest<Response<ObtemPipelineComEtapasResult>>
    {
        public Guid PipelineId { get; set; }
        public Guid EmpresaId { get; set; }
        public ObtemPipelineComEtapasCommand(Guid pipelineId, Guid empresaId)
        {
            PipelineId = pipelineId;
            EmpresaId = empresaId;
        }
    }
}
