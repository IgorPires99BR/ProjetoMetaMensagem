namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    // Desfecho da tentativa de reservar uma conversa para processar o Flow.
    public enum ResultadoReserva
    {
        // Esta chamada ficou com a reserva: pode processar e responder.
        Reservada,

        // Outra mensagem do mesmo cliente esta sendo processada agora. Desiste sem responder --
        // e assim que o bot manda uma resposta so quando a pessoa dispara varias mensagens.
        JaEmProcessamento,

        // Ainda nao existe conversa para este contato: segue para o caminho de criacao, onde a
        // disputa e resolvida pelo indice unico UX_EstadoConversa_Ativa.
        SemConversaAinda
    }
}
