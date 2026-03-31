using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbSession _session;

        public ICompaniesRepository CompaniesRepository { get; set; }
        public IFlowsRepository FlowsRepository { get; set; }
        public IConversationsRepository ConversationsRepository { get; set; }

        public UnitOfWork(DbSession session,
            ICompaniesRepository companiesRepository,
            IFlowsRepository flowsRepository,
            IConversationsRepository conversationsRepository)
        {
            _session = session;
            CompaniesRepository = companiesRepository;
            FlowsRepository = flowsRepository;
            ConversationsRepository = conversationsRepository;
        }

        public void Commit()
        {
            _session.Transaction.Commit();
            Dispose();
        }

        public void Rollback()
        {
            _session.Transaction.Rollback();
            Dispose();
        }

        public void BeginTransaction()
        {
            _session.Transaction = _session._connection.BeginTransaction();
        }

        public void Dispose()
        {
            _session.Transaction?.Dispose();
        }
    }
}
