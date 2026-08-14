using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class CampanhaContato
    {
        // Gravado no momento em que o worker "pega" o contato, ANTES de chamar a Meta, e
        // sobrescrito pelo resultado real logo depois. Se o processo morrer no meio do envio,
        // e este texto que sobra: marca o contato como ja tentado (nao sera reenviado, pra nao
        // duplicar mensagem paga e derrubar a nota do numero) e deixa o caso visivel no
        // relatorio pra decisao manual, em vez de sumir em silencio.
        public const string EnvioInterrompido = "Envio interrompido: o processo caiu durante o disparo. Confira na Meta se a mensagem saiu antes de reenviar.";

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CampanhaId { get; set; }

        [Required]
        public Guid ContatoId { get; set; }

        public bool Processado { get; set; }

        public bool? Sucesso { get; set; }

        public string? MensagemErro { get; set; }
    }
}
