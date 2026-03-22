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
    }
}
