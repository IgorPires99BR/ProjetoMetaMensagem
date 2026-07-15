using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IContatoRepository
    {
        Task Incluir(Contato contato);
        Task Alterar(Contato contato);
        Task Excluir(string id);
        Task<Contato?> ObterPorId(int id);
        Task<IEnumerable<Contato>> Obter();
        Task<Contato?> ObterPorTelefone(Guid empresaId, string telefone);
        Task<IEnumerable<Contato>> ObterPorUsuario(Guid usuarioId);
    }
}
