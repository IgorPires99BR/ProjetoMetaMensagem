using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IMensagemRecebidaRepository
    {
        Task Incluir(MensagemRecebida mensagem);
        Task<List<MensagemRecebida>> ListarPorEmpresa(Guid empresaId);
        Task<List<MensagemRecebida>> ListarPorContato(Guid empresaId, Guid contatoId);
        Task<List<MensagemRecebida>> ListarPorContatoPaginado(Guid empresaId, Guid contatoId, int pagina, int tamanhoPagina);
        Task MarcarComoLida(Guid id);
        Task MarcarTodasComoLidas(Guid empresaId, Guid contatoId);
        Task<int> ContarNaoLidas(Guid empresaId, Guid contatoId);
    }
}
