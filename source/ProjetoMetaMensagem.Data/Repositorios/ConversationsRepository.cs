using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ConversationsRepository : IConversationsRepository
    {

        private readonly DbSession _session;

        public ConversationsRepository(DbSession session)
        {
            _session = session;
        }

        public Task<int> Incluir(Conversations conversations)
        {
            throw new NotImplementedException();
        }

        public Task<List<Conversations>> Obter()
        {
            throw new NotImplementedException();
        }
    }
}
