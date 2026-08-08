using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ITagRepository
    {
        Task<Guid> Incluir(Tag tag);
        // empresaIdSolicitante restringe a operacao ao acervo da empresa informada.
        // null = administrador (sem restricao). Tag tem EmpresaId proprio; ContatoTag nao,
        // e chega na empresa pela Tag e pelo Contato (que passa por Usuario).
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<IEnumerable<Tag>> ListarPorEmpresa(Guid empresaId);
        Task<Tag?> ObterPorId(Guid id);
        Task<IEnumerable<Tag>> ObterPorContato(Guid contatoId);
        Task AssociarTagsContato(Guid contatoId, List<Guid> tagIds, Guid? empresaIdSolicitante);
    }
}
