using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    // O que o cliente final ve no WhatsApp ao abrir o contato do numero que envia.
    public class PerfilNumeroMetaDto
    {
        public string? Sobre { get; set; }
        public string? Descricao { get; set; }
        public string? Endereco { get; set; }
        public string? Email { get; set; }
        public List<string> Sites { get; set; } = new();
        public string? Segmento { get; set; }
        public string? FotoUrl { get; set; }

        // Nome exibido hoje (verified_name) e o pedido de troca em analise, se houver.
        public string? NomeExibido { get; set; }
        public string? NovoNomeSolicitado { get; set; }
        public string? StatusNovoNome { get; set; }
    }

    // Campos nulos ficam de fora do envio, pra Meta manter o valor atual.
    public class PerfilNumeroEnvio
    {
        public string? Sobre { get; set; }
        public string? Descricao { get; set; }
        public string? Endereco { get; set; }
        public string? Email { get; set; }
        public List<string>? Sites { get; set; }
        public string? Segmento { get; set; }
        public string? FotoHandle { get; set; }
    }
}
