using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IHistoricoDisparoRepository
    {
        Task Incluir(HistoricoDisparo historicoDisparo);
        Task<HistoricoDisparo?> ObterPorId(Guid id);

        Task<IEnumerable<HistoricoDisparo>> ListarPorContato(Guid empresaId, Guid contatoId);
        Task<IEnumerable<HistoricoDisparo>> ListarPorContatoPaginado(Guid empresaId, Guid contatoId, int pagina, int tamanhoPagina);
        Task<IEnumerable<HistoricoDisparoComTelefone>> ListarPorEmpresa(Guid empresaId);
        Task<HistoricoDisparo?> ObterPorWamidMeta(string wamidMeta);
        Task AtualizarStatusEntregaPorWamid(string wamid, string status, string? motivoFalha = null);
    }
}
