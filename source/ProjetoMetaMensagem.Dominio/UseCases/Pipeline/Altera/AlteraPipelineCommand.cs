using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Altera
{
    public class AlteraPipelineCommand : IRequest<Response<AlteraPipelineResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem esse escopo o
        // UPDATE casava so pelo Id e permitia renomear pipeline de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }

    public class AlteraPipelineResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
