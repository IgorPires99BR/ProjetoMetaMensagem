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
        // null = administrador (sem restricao, ve contatos de todas as empresas).
        Task<IEnumerable<Contato>> ObterPorEmpresa(Guid? empresaId);
        // Busca em lote pra lista de chats: evita 1 consulta por contato ao montar a tela.
        // Escopado por empresa pelo mesmo motivo do ObterPorTelefone -- Contato so tem o vinculo
        // com a empresa via Usuario.
        Task<IEnumerable<Contato>> ObterPorIds(Guid empresaId, IEnumerable<Guid> ids);
    }
}
