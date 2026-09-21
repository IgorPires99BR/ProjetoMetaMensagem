namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    public class ResultadoEnvioTemplate
    {
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
