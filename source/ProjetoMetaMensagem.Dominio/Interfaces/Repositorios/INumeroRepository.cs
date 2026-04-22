using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface INumeroRepository
    {
        Task Incluir(Numero numero);
        Task Alterar(Numero numero);
        Task Excluir(string id);
        Task<Numero?> ObterPorId(int id);
        Task<IEnumerable<Numero>> Obter();
        Task<IEnumerable<Numero>> ObterPorUsuario(string usuarioId);
    }
}
