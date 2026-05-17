using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Contato
    {
        public Contato()
        {
            
        }

        public Contato(CriaContatoCommand command)
        {
            Id = Guid.NewGuid();
            UsuarioId = command.UsuarioId;
            Telefone = command.Telefone;
            Nome = command.Nome;
            Email = command.Email;
            DataCriacao = DateTime.Now;
        }

        public Contato(AlteraContatoCommand command)
        {
            Id = command.Id;
            UsuarioId = command.UsuarioId;
            Telefone = command.Telefone;
            Nome = command.Nome;
            Email = command.Email;
            DataCriacao = DateTime.Now;
        }
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required] // Único obrigatório conforme regra de negócio
        [MaxLength(50)]
        public string Telefone { get; set; }

        [MaxLength(255)]
        public string? Nome { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
