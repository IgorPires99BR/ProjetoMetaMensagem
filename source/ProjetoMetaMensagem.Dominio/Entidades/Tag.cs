using ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Tag
    {
        public Tag()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
        }

        public Tag(CriaTagCommand command) : this()
        {
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Cor = command.Cor ?? "#3D6EE8";
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(7)]
        public string Cor { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
