using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Template
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(255)]
        public string NomeTemplate { get; set; }

        [Required]
        public string Conteudo { get; set; }

        [MaxLength(100)]
        public string? Categoria { get; set; }

        [MaxLength(10)]
        public string Idioma { get; set; } = "pt_BR";

        [MaxLength(50)]
        public string? Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
