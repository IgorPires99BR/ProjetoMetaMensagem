namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Linha crua da consulta unificada (MensagemRecebida UNION HistoricoDisparo) usada pra
    // paginar a conversa do Chat na ordem real, sem o desvio de paginar cada tabela separado.
    public class ItemConversaUnificado
    {
        public Guid Id { get; set; }
        public string Origem { get; set; } = string.Empty; // "user" ou "bot"
        public string? Texto { get; set; }
        public DateTime Data { get; set; }
        public string? Wamid { get; set; }
        public string? Status { get; set; }
        public string? Erro { get; set; }
        public string? MidiaId { get; set; }
        public string? TipoMidia { get; set; }
    }
}
