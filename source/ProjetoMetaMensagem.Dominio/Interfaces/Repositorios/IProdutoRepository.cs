using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IProdutoRepository
    {
        Task<Guid> Incluir(Produto produto);
        Task Alterar(Produto produto);
        Task Excluir(Guid id);
        Task<Produto?> ObterPorId(Guid id);
        Task<IEnumerable<Produto>> Obter();
        Task<IEnumerable<Produto>> ListarPorEmpresa(Guid empresaId);
    }
}
