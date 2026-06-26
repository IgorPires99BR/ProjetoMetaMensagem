namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class ConversationState
    {
        public ConversationState()
        {
            Id = Guid.NewGuid();
            DataInicio = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid FlowId { get; set; }
        public Guid? EtapaAtualId { get; set; }
        public string? Variaveis { get; set; }  // JSON com variaveis capturadas (ex: {"nome":"Joao"})
        public DateTime DataInicio { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Finalizado { get; set; }
    }
}
