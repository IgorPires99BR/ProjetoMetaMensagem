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

        // Ate quando esta conversa esta reservada por um processamento em andamento. Enquanto
        // estiver no futuro, outra mensagem do mesmo cliente e ignorada pelo Flow -- e assim que
        // o bot responde uma vez so quando a pessoa manda varias mensagens seguidas.
        public DateTime? ProcessandoAte { get; set; }

        // Quantas vezes seguidas o cliente respondeu algo que a etapa atual nao esperava.
        // Zera sempre que ele acerta e o flow avanca.
        public int TentativasNaEtapa { get; set; }

        // O bot desistiu de conduzir esta conversa e passou pra uma pessoa.
        public bool AguardandoAtendente { get; set; }
    }
}
