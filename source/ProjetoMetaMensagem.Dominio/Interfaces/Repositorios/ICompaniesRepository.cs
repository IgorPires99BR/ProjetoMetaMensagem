using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ICompaniesRepository
    {
        Task<int> Incluir(Companies empresa);
        Task<Companies> Login(string email, string senha);
        Task Alterar(int companyId, Companies command);
        Task Deletar(int companyId);
        Task<List<Companies>> Obter();
    }
}
