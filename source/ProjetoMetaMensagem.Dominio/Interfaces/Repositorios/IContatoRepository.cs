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
        // Apaga, ou so inativa quando o contato ja tem historico (disparo, conversa, cobranca).
        Task<ResultadoExclusaoContato> Excluir(string id, Guid? empresaIdSolicitante);
        // Devolve 0 se o contato nao existe, e de outra empresa ou ja esta ativo.
        Task<int> Reativar(Guid id, Guid? empresaIdSolicitante);
        Task<Contato?> ObterPorTelefone(Guid empresaId, string telefone);
        // null = administrador (sem restricao).
        Task<Contato?> ObterPorId(Guid id, Guid? empresaIdSolicitante);
        // null = administrador (sem restricao, ve contatos de todas as empresas).
        Task<IEnumerable<Contato>> ObterPorEmpresa(Guid? empresaId, bool apenasInativos = false);
        // Busca em lote pra lista de chats: evita 1 consulta por contato ao montar a tela.
        // Escopado por empresa pelo mesmo motivo do ObterPorTelefone -- Contato so tem o vinculo
        // com a empresa via Usuario.
        Task<IEnumerable<Contato>> ObterPorIds(Guid empresaId, IEnumerable<Guid> ids);
    }

    public enum ResultadoExclusaoContato
    {
        NaoEncontrado = 0,
        Excluido = 1,
        Inativado = 2
    }
}
