using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.RemoverLead
{
    public class RemoverLeadCommand : IRequest<Response<bool>>
    {
        public Guid LeadId { get; set; }
        public RemoverLeadCommand(Guid leadId) => LeadId = leadId;

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar lead alheio mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
