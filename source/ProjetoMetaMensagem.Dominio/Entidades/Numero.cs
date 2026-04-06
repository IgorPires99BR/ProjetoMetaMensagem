using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Numero
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroTelefone { get; set; }

        [MaxLength(100)]
        public string? Descricao { get; set; }

        [MaxLength(255)]
        public string? InstanciaId { get; set; }
    }
}
