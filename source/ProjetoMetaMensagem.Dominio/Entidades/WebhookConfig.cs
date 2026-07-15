using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class WebhookConfig
    {
        public WebhookConfig()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            Ativo = true;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; set; }

        [Required]
        [MaxLength(100)]
        public string Evento { get; set; }

        [MaxLength(500)]
        public string? TokenSegredo { get; set; }

        public bool Ativo { get; set; }

        public DateTimeOffset DataCriacao { get; set; }
    }
}
