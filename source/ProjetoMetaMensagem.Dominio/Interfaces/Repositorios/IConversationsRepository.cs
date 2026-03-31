using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IConversationsRepository
    {
        Task<List<Conversations>> Obter(string companyId);

        Task<int> Incluir(Conversations conversations);
    }
}
