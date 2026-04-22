using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IUsuarioRepository
    {
        Task Incluir(Usuario usuario);
        Task Alterar(Usuario usuario);

        Task<Usuario?> Logar(string email, string senhaHash);

        Task Excluir(string id);
        Task<Usuario?> ObterPorId(Guid id);
        Task<IEnumerable<Usuario>> Obter();
        // Método adicional comum para usuários
        Task<IEnumerable<Usuario>> ObterPorEmpresa(Guid empresaId);
    }
}
