namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead
{
    public class AdicionarLeadResult
    {
        public Guid Id { get; set; }
        public Guid NovaEtapaId { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
    }
}
