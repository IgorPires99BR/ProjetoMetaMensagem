using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IConversationStateRepository
    {
        Task<ConversationState?> ObterPorEmpresaEContato(Guid empresaId, Guid contatoId);
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
