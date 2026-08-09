namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    public class NumeroMetaDto
    {
        public string Id { get; set; }
        public string NumeroFormatado { get; set; }
        public string NomeVerificado { get; set; }
        public string Status { get; set; }
        public string Qualidade { get; set; }
        public string CodigoPais { get; set; }
        public bool EhContaOficial { get; set; }
    }
}
