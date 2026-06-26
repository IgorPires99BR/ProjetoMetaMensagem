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
        IFlowRepository Flow { get; }
        IConversationsRepository ConversationsRepository { get; }
        IEmpresaRepository Empresa { get; }
        IUsuarioRepository Usuario { get; }
        IContatoRepository Contato { get; }
        INumeroRepository Numero { get; }
        ITemplateRepository Template { get; }
        IHistoricoDisparoRepository HistoricoDisparo { get; }
        IConversationStateRepository ConversationState { get; }
        void Commit();
        void BeginTransaction();
        void Rollback();
        
    }
}
