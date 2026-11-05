using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IDisparoLoteRepository
    {
        Task Incluir(DisparoLote lote, IEnumerable<DisparoLoteItem> itens);
        Task<DisparoLote?> ObterPorChave(Guid empresaId, Guid chaveIdempotencia);
        Task<DisparoLote?> ObterPorId(Guid id);

        // Contatos com este template ainda na fila: reenviar agora cobraria duas vezes.
        Task<IEnumerable<Guid>> ObterContatosJaNaFila(Guid templateId, IEnumerable<Guid> contatoIds);

        // Reservas atomicas: dois workers (ou um deploy sobreposto) nunca pegam o mesmo item.
        Task<DisparoLoteItem?> ReservarProximo(int segundosDeReserva);
        Task<DisparoLoteItem?> ReservarEnvioInterrompido(int segundosDeReserva);
        Task<int> LiberarPreparacoesInterrompidas(int maximoDeTentativas);

        Task SalvarPreparacao(Guid itemId, string envioJson, string? cobrancaJson);
        // Devolve 0 se o item nao estava mais em preparacao: quem chamou nao pode enviar.
        Task<int> MarcarEnviando(Guid itemId, int segundosDeReserva);
        Task AgendarNovaTentativa(Guid itemId, int segundos, string? erro);
        Task Finalizar(Guid itemId, string status, string? erro);

        Task<IEnumerable<DisparoLoteResumo>> ListarRecentes(Guid empresaId, int quantidade);
        Task<DisparoLoteResumo?> ObterResumo(Guid loteId, Guid? empresaIdSolicitante);
        Task<IEnumerable<DisparoLotePendencia>> ObterPendencias(Guid loteId);
    }
}
