using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto
{
    public class AlteraProdutoCommand : IRequest<Response<AlteraProdutoResult>>
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? ImagemUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? Categoria { get; set; }
        public bool Ativo { get; set; }
    }
}
