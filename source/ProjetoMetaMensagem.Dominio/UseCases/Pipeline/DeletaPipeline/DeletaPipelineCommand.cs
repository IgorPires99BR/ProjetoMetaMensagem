using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.DeletaPipeline
{
    public class DeletaPipelineCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public DeletaPipelineCommand(Guid id) => Id = id;

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar pipeline alheio mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
