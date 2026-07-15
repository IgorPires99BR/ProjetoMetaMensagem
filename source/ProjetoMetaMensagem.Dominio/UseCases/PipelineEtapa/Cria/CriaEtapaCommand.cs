using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Cria
{
    public class CriaEtapaCommand : IRequest<Response<CriaEtapaResult>>
    {
        public Guid PipelineId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = "#3D6EE8";
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
    }

    public class CriaEtapaResult
    {
        public Guid Id { get; set; }
        public Guid PipelineId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }
}
