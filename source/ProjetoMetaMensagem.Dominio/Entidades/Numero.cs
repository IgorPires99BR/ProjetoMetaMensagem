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
            UsuarioId = command.UsuarioId;
            Telefone = command.NumeroTelefone;
            Descricao = command.Descricao;
            InstanciaId = command.InstanciaId;
            DataCriacao = DateTime.Now;
        }

        public Numero(AlteraNumeroCommand command)
        {
            Id = command.Id;
            UsuarioId = command.UsuarioId;
            Telefone = command.NumeroTelefone;
            Descricao = command.Descricao;
            InstanciaId = command.InstanciaId;
            DataAtualizacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string Telefone { get; set; }

        [MaxLength(100)]
        public string? Descricao { get; set; }

        [MaxLength(255)]
        public string? InstanciaId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
