using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class TemplateConexao
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        public Guid TemplateOrigemId { get; set; }

        [Required]
        public Guid TemplateDestinoId { get; set; }

        [MaxLength(100)]
        public string? BotaoTexto { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
