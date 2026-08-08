namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas
{
    public class ObtemPipelineComEtapasResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<EtapaComLeads> Etapas { get; set; } = new();
    }

    public class EtapaComLeads
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = string.Empty;
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
        public List<LeadNaEtapa> Leads { get; set; } = new();
    }

    public class LeadNaEtapa
    {
        public Guid Id { get; set; }
        public Guid ContatoId { get; set; }
        public string NomeContato { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public decimal? Valor { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataEntrada { get; set; }
    }
}
