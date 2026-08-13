using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa
{
    public class ListaEtapaCommand : IRequest<Response<List<ListaEtapaResult>>>
    {
        public Guid PipelineId { get; set; }
        public ListaEtapaCommand(Guid pipelineId) => PipelineId = pipelineId;

        // Escopo vem do token, nunca da rota: senao qualquer usuario autenticado listava as
        // etapas de um pipeline de OUTRA empresa so sabendo/adivinhando o PipelineId.
        // null = administrador (sem restricao).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
