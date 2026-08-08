using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IProdutoRepository
    {
        Task<Guid> Incluir(Produto produto);
        // empresaIdSolicitante restringe a operacao aos produtos da empresa informada.
        // null = administrador (sem restricao). Produto tem EmpresaId proprio.
        Task<int> Alterar(Produto produto, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Produto?> ObterPorId(Guid id);
        Task<IEnumerable<Produto>> Obter();
        Task<IEnumerable<Produto>> ListarPorEmpresa(Guid empresaId);
    }
}
