using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Lista
{
    public class ListaPipelineCommand : IRequest<Response<List<ListaPipelineResult>>>
    {
        public Guid EmpresaId { get; set; }
        public ListaPipelineCommand(Guid empresaId) => EmpresaId = empresaId;
    }

    public class ListaPipelineResult
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int TotalEtapas { get; set; }
        public int TotalLeads { get; set; }
    }
}
