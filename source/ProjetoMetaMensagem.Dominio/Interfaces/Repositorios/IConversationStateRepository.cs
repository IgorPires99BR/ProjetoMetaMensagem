using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IConversationStateRepository
    {
        Task<ConversationState?> ObterPorEmpresaEContato(Guid empresaId, Guid contatoId);

        // Igual ao de cima, mas travando a linha ate o fim da transacao. Usado so pelo
        // orquestrador de Flow: quando o cliente manda varias mensagens seguidas, as
        // requisicoes concorrentes liam a MESMA etapa atual e todas avancavam o flow a partir
        // dela -- o cliente recebia a mesma resposta do bot repetida. Com a trava, a segunda
        // espera a primeira terminar e ja le a etapa atualizada.
        Task<ConversationState?> ObterAtivaParaAtualizacao(Guid empresaId, Guid contatoId);
        Task Incluir(ConversationState state);
        Task Atualizar(ConversationState state);
        Task<List<ConversationState>> ObterPorFlow(Guid flowId);
        Task Excluir(Guid id);

        // Batch lookup (sem N+1) para a lista de chats saber, por contato, se ha flow ativo e se foi assumido.
        Task<List<ConversationState>> ObterAtivasPorEmpresaEContatos(Guid empresaId, List<Guid> contatoIds);
        Task AssumirManualmente(Guid id, Guid usuarioId);
        Task DevolverAoBot(Guid id);
    }
}
