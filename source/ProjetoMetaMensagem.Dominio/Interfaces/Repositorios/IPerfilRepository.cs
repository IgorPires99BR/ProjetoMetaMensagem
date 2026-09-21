using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IPerfilRepository
    {
        Task Incluir(Perfil perfil);
        Task<int> Alterar(Perfil perfil, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Perfil?> ObterPorId(Guid id, Guid? empresaIdSolicitante);
        Task<IEnumerable<Perfil>> ObterPorEmpresa(Guid? empresaId);

        // Usado no login pra resolver quais telas o usuario enxerga. null = usuario sem
        // perfil atribuido (comportamento legado no front).
        Task<List<string>?> ObterTelasDoPerfil(Guid? perfilId);
    }
}
