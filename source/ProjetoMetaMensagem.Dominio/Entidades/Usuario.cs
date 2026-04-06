using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Usuario
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public Empresa Empresa { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.Now;
    }
}
