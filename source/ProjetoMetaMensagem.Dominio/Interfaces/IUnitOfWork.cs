using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICompaniesRepository CompaniesRepository { get; }
        IFlowsRepository FlowsRepository { get; }
        IConversationsRepository ConversationsRepository { get; }
        void Commit();
        void BeginTransaction();
        void Rollback();
        
    }
}
