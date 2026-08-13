using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa
{
    public class CriaEtapaCommand : IRequest<Response<CriaEtapaResult>>
    {
        public Guid PipelineId { get; set; }

        // Escopo vem do token, nunca do corpo: senao dava pra injetar uma etapa dentro do
        // pipeline de OUTRA empresa so sabendo o PipelineId. null = administrador.
        public Guid? EmpresaIdSolicitante { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = "#3D6EE8";
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
    }
}
