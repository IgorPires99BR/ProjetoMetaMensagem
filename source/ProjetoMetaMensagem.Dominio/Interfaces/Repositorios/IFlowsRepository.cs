using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IFlowsRepository
    {
        Task<List<Flows>> Obtem(string companyId);
        Task<int> Incluir(Flows flow);
    }
}
