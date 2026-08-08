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

        // Preenchido pelo controller a partir do JWT (null = administrador). Nao confundir com
        // EmpresaId, que vem do corpo e por isso o atacante escolhe. Sem esse escopo o UPDATE
        // casava so pelo Id e permitia alterar produto de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
