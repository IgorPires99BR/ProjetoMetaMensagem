using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto
{
    public class DeletaProdutoCommand : IRequest<Response<DeletaProdutoResult>>
    {
        public Guid Id { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar produto alheio mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
