using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IMensagemRecebidaRepository
    {
        Task Incluir(MensagemRecebida mensagem);
        Task<List<MensagemRecebida>> ListarPorEmpresa(Guid empresaId);
        Task<List<MensagemRecebida>> ListarPorContato(Guid empresaId, Guid contatoId);
        Task MarcarComoLida(Guid id);
        Task<int> ContarNaoLidas(Guid empresaId, Guid contatoId);
    }
}
