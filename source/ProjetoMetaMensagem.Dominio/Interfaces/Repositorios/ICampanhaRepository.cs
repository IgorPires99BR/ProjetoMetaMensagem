using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ICampanhaRepository
    {
        Task<Guid> Incluir(Campanha campanha);
        Task IncluirContatos(List<CampanhaContato> contatos);
        Task<IEnumerable<Campanha>> Listar(Guid empresaId);
        Task<IEnumerable<Campanha>> ObterPendentes();
        Task AtualizarStatus(Guid id, string status);
        Task<Campanha?> ObterPorId(Guid id);
        Task<IEnumerable<CampanhaContato>> ObterContatosPorCampanha(Guid campanhaId);
    }
}
