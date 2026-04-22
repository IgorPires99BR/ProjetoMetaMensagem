using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Numero
    {

        public Numero()
        {
            
        }

        public Numero(CriaNumeroCommand command)
        {
            Id = Guid.NewGuid().ToString();
            UsuarioId = command.UsuarioId;
            NumeroTelefone = command.NumeroTelefone;
            Descricao = command.Descricao;
            InstanciaId = command.InstanciaId;
        }

        public Numero(AlteraNumeroCommand command)
        {
            Id = Guid.NewGuid().ToString();
            UsuarioId = command.UsuarioId;
            NumeroTelefone = command.NumeroTelefone;
            Descricao = command.Descricao;
            InstanciaId = command.InstanciaId;
        }

        [Key]
        public string Id { get; set; }

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
