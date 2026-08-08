namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead
{
    public class MoverLeadResult
    {
        public Guid Id { get; set; }
        public Guid NovaEtapaId { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
    }
}
