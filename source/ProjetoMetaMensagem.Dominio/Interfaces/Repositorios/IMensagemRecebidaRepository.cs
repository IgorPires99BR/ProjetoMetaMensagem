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
        // Usado pra confirmar que um MidiaId pertence a empresa antes de baixar a midia da Meta
        // usando o token dela -- sem isso, qualquer MidiaId (de qualquer empresa) era aceito.
        Task<bool> ExisteMidiaId(Guid empresaId, string midiaId);
    }
}
