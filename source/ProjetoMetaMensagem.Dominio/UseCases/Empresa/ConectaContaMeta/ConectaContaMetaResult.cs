namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConectaContaMeta
{
    public class ConectaContaMetaResult
    {
        public bool Sucesso { get; set; }
        public string? WabaId { get; set; }
        public string? PhoneNumberId { get; set; }
        public bool AppAssinado { get; set; }
    }
}
