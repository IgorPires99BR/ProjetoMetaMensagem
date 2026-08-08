namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa
{
    public class CriaEtapaResult
    {
        public Guid Id { get; set; }
        public Guid PipelineId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }
}
