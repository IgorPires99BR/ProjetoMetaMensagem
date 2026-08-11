using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface INumeroRepository
    {
        Task Incluir(Numero numero);
        // empresaIdSolicitante restringe a operacao aos numeros da empresa informada.
        // null = administrador (sem restricao). Numero nao tem EmpresaId proprio: o vinculo
        // com a empresa passa por Usuario.
        Task<int> Alterar(Numero numero, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Numero?> ObterPorId(int id);
        Task<Numero?> ObterPorId(Guid id);
        // InstanciaId = phone_number_id da Meta. Usado pra resolver qual Numero especifico
        // recebeu um evento do webhook (varios numeros podem existir na mesma empresa).
        Task<Numero?> ObterPorInstanciaId(string instanciaId);
        Task<IEnumerable<Numero>> Obter();
        Task<IEnumerable<Numero>> ObterPorUsuario(Guid usuarioId);
    }
}
