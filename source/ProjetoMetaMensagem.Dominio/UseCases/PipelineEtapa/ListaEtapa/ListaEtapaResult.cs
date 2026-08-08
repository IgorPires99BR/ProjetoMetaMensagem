namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa
{
    public class ListaEtapaResult
    {
        public Guid Id { get; set; }
        public Guid PipelineId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = string.Empty;
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
        public int TotalLeads { get; set; }
    }
}
