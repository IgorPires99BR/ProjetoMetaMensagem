using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.Mover
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

    public class MoverLeadResult
    {
        public Guid Id { get; set; }
        public Guid NovaEtapaId { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
    }

    public class AdicionarLeadCommand : IRequest<Response<MoverLeadResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid PipelineEtapaId { get; set; }
        public decimal? Valor { get; set; }
        public string? Observacao { get; set; }
    }

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
