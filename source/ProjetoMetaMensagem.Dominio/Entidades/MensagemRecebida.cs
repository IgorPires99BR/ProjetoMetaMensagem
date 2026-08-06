namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class MensagemRecebida
    {
        public MensagemRecebida()
        {
            Id = Guid.NewGuid();
            DataRecebimento = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid? ContatoId { get; set; }
        public string TelefoneRemetente { get; set; }
        public string Conteudo { get; set; }
        public string Tipo { get; set; } = "recebida"; // recebida, enviada
        public DateTime DataRecebimento { get; set; }
        public bool Lida { get; set; }
        public Guid? FlowId { get; set; }
        public string? MidiaId { get; set; }
        public string? TipoMidia { get; set; }
    }
}
