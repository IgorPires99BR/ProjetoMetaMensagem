using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto
{
    public class DeletaProdutoCommand : IRequest<Response<DeletaProdutoResult>>
    {
        public Guid Id { get; set; }
    }
}
