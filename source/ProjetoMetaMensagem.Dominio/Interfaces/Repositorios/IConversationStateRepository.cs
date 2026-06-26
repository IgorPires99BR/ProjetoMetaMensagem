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
    }
}
