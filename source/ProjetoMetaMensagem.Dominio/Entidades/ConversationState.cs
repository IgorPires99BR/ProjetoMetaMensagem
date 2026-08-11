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

        // Null = flow tocando normalmente; preenchido = vendedor assumiu a conversa manualmente
        // e o FlowOrchestratorService para de avancar etapa ate ser devolvida ao bot.
        public Guid? AssumidoPorUsuarioId { get; set; }
        public DateTime? DataAssumido { get; set; }
    }
}
