using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Altera
{
    public class AlteraEtapaCommand : IRequest<Response<AlteraEtapaResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = "#3D6EE8";
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
    }

    public class AlteraEtapaResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
