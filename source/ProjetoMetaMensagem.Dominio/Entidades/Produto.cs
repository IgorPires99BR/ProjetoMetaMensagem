using ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Produto
    {
        public Produto()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            Ativo = true;
        }

        public Produto(CriaProdutoCommand command) : this()
        {
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Descricao = command.Descricao;
            Preco = command.Preco;
            ImagemUrl = command.ImagemUrl;
            LinkUrl = command.LinkUrl;
            Categoria = command.Categoria;
        }

        public Produto(AlteraProdutoCommand command)
        {
            Id = command.Id;
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Descricao = command.Descricao;
            Preco = command.Preco;
            ImagemUrl = command.ImagemUrl;
            LinkUrl = command.LinkUrl;
            Categoria = command.Categoria;
            Ativo = command.Ativo;
            DataCriacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        public decimal Preco { get; set; }

        [MaxLength(500)]
        public string? ImagemUrl { get; set; }

        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        [MaxLength(100)]
        public string? Categoria { get; set; }

        public bool Ativo { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
