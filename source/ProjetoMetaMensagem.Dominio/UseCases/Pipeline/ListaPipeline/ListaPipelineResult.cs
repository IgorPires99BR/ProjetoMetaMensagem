namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline
{
    public class ListaPipelineResult
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int TotalEtapas { get; set; }
        public int TotalLeads { get; set; }
    }
}
