using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead
{
    public class MoverLeadCommand : IRequest<Response<MoverLeadResult>>
    {
        public Guid LeadId { get; set; }
        public Guid NovaEtapaId { get; set; }
        public decimal? Valor { get; set; }
        public string? Observacao { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem esse escopo o
        // UPDATE casava so pelo Id e permitia mover lead de outra empresa -- ou jogar um lead
        // proprio dentro do funil de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
