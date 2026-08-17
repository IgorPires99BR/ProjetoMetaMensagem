using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // De qual anúncio veio o lead que chegou pelo WhatsApp. Preenchido na primeira mensagem de
    // quem clicou num anúncio Click-to-WhatsApp -- é a única vez que a Meta manda esse dado.
    public class OrigemLead
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid? ContatoId { get; set; }
        public string Telefone { get; set; } = string.Empty;

        // Identificador do clique, exigido pela Conversions API para creditar a conversão
        public string? CtwaClid { get; set; }
        public string? SourceId { get; set; }
        public string? SourceType { get; set; }
        public string? SourceUrl { get; set; }
        public string? Headline { get; set; }
        public string? Corpo { get; set; }

        public DateTime DataPrimeiroContato { get; set; }
        public bool ConversaoEnviada { get; set; }

        public OrigemLead()
        {
            Id = Guid.NewGuid();
            DataPrimeiroContato = DateTime.Now;
        }
    }
}
