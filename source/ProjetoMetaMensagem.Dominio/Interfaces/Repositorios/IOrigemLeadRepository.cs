using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IOrigemLeadRepository
    {
        Task Incluir(OrigemLead origem);
        Task<OrigemLead?> ObterPorTelefone(Guid empresaId, string telefone);
        Task<IEnumerable<OrigemLead>> ListarPorEmpresa(Guid empresaId);
        Task MarcarConversaoEnviada(Guid id);
    }
}
