using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead
{
    public class AdicionarLeadCommand : IRequest<Response<AdicionarLeadResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid PipelineEtapaId { get; set; }
        public decimal? Valor { get; set; }
        public string? Observacao { get; set; }
    }
}
