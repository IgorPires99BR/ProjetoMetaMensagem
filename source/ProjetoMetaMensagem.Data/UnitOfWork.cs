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
        public IFlowRepository Flow { get; set; }
        public IConversationsRepository ConversationsRepository { get; set; }
        public IEmpresaRepository Empresa { get; set; }
        public IUsuarioRepository Usuario { get; set; }
        public IContatoRepository Contato { get; set; }
        public INumeroRepository Numero{ get; set; }
        public ITemplateRepository Template { get; set; }
        public IHistoricoDisparoRepository HistoricoDisparo { get; set; }
        public IConversationStateRepository ConversationState { get; set; }
        public ITagRepository Tag { get; set; }
        public ICampanhaRepository Campanha { get; set; }
        public IWebhookConfigRepository WebhookConfig { get; set; }
        public IProdutoRepository Produto { get; set; }
        public IPipelineRepository Pipeline { get; set; }

        public UnitOfWork(DbSession session,
            ICompaniesRepository companiesRepository,
            IFlowsRepository flowsRepository,
            IConversationsRepository conversationsRepository,
            IEmpresaRepository empresaRepository,
            IUsuarioRepository usuarioRepository,
            IContatoRepository contatoRepository,
            INumeroRepository numeroRepository,
            ITemplateRepository templateRepository,
            IHistoricoDisparoRepository historicoDisparo,
            IFlowRepository flow,
            IConversationStateRepository conversationState,
            ITagRepository tagRepository,
            ICampanhaRepository campanhaRepository,
            IWebhookConfigRepository webhookConfigRepository,
            IProdutoRepository produtoRepository,
            IPipelineRepository pipelineRepository)
        {
            _session = session;
            CompaniesRepository = companiesRepository;
            FlowsRepository = flowsRepository;
            ConversationsRepository = conversationsRepository;
            Empresa = empresaRepository;
            Usuario = usuarioRepository;
            Contato = contatoRepository;
            Numero = numeroRepository;
            Template = templateRepository;
            HistoricoDisparo = historicoDisparo;
            Flow = flow;
            ConversationState = conversationState;
            Tag = tagRepository;
            Campanha = campanhaRepository;
            WebhookConfig = webhookConfigRepository;
            Produto = produtoRepository;
            Pipeline = pipelineRepository;
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
