using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Campanha
    {
        public Campanha()
        {
            Id = Guid.NewGuid();
            Status = "AGENDADA";
            DataCriacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; }

        public Guid? TemplateId { get; set; }

        public string? ConteudoLivre { get; set; }

        public DateTime DataAgendamento { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }

        public int TotalContatos { get; set; }

        public int Processados { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
