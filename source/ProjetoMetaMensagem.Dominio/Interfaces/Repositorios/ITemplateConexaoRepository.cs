using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ITemplateConexaoRepository
    {
        Task Incluir(TemplateConexao templateConexao);
        Task<IEnumerable<TemplateConexao>> ListarPorEmpresa(Guid empresaId);
        // empresaIdSolicitante restringe a exclusao as conexoes da empresa informada.
        // null = administrador (sem restricao). TemplateConexao tem EmpresaId proprio.
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
    }
}
