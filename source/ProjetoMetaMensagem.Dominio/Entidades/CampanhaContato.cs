using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class CampanhaContato
    {
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
