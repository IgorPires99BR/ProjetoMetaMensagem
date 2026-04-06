using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IEmpresaRepository
    {
        Task Incluir(Empresa empresa);
        Task Alterar(Empresa empresa);
        Task Excluir(string id);
        Task<Empresa?> ObterPorId(string id);
        Task<IEnumerable<Empresa>> Obter();
    }
}
