using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto
{
    public class ListaProdutoResult
    {
        public ListaProdutoResult(Entidades.Produto produto)
        {
            Id = produto.Id;
            EmpresaId = produto.EmpresaId;
            Nome = produto.Nome;
            Descricao = produto.Descricao;
            Preco = produto.Preco;
            ImagemUrl = produto.ImagemUrl;
            LinkUrl = produto.LinkUrl;
            Categoria = produto.Categoria;
            Ativo = produto.Ativo;
            DataCriacao = produto.DataCriacao;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? ImagemUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? Categoria { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
