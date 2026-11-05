namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    // O que uma falha de envio permite fazer. Repetir um envio que pode ter saido duplica a
    // mensagem (e a Meta nao tem chave de idempotencia para evitar).
    public enum TipoFalhaEnvio
    {
        // Recusa clara: repetir daria o mesmo resultado (numero invalido, template errado).
        Definitiva,
        // Com certeza nao saiu: da para tentar de novo (limite de envio, sem conexao).
        Transitoria,
        // Pode ter saido: timeout ou erro interno depois de o pedido chegar la.
        Incerta
    }

    public class ResultadoEnvioTemplate
    {
        public TipoFalhaEnvio TipoFalha { get; set; } = TipoFalhaEnvio.Definitiva;

        public bool Sucesso { get; set; }
        public string WamidMeta { get; set; }
        public string Erro { get; set; }
        public string? JsonEnviado { get; set; }
        public string ContatoId { get; set; }

        // Telefone da posicao do lote a que este resultado se refere (o telefone pode se repetir
        // entre contatos diferentes, entao ele sozinho nao identifica o destinatario).
        public string? Telefone { get; set; }
    }
}
