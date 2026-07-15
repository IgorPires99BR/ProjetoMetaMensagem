using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ITagRepository
    {
        Task<Guid> Incluir(Tag tag);
        Task Excluir(Guid id);
        Task<IEnumerable<Tag>> ListarPorEmpresa(Guid empresaId);
        Task<Tag?> ObterPorId(Guid id);
        Task<IEnumerable<Tag>> ObterPorContato(Guid contatoId);
        Task AssociarTagsContato(Guid contatoId, List<Guid> tagIds);
    }
}
