using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.DeletaEtapa
{
    public class DeletaEtapaCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public DeletaEtapaCommand(Guid id) => Id = id;

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar etapa alheia mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
