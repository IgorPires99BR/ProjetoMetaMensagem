using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IContatoRepository
    {
        Task Incluir(Contato contato);
        // empresaIdSolicitante restringe a operacao aos contatos da empresa informada.
        // null = administrador (sem restricao). Contato nao tem EmpresaId proprio: o vinculo
        // com a empresa passa por Usuario.
        Task<int> Alterar(Contato contato, Guid? empresaIdSolicitante);
        Task<int> Excluir(string id, Guid? empresaIdSolicitante);
        Task<Contato?> ObterPorId(int id);
        Task<IEnumerable<Contato>> Obter();
        Task<Contato?> ObterPorTelefone(Guid empresaId, string telefone);
        Task<IEnumerable<Contato>> ObterPorUsuario(Guid usuarioId);
    }
}
