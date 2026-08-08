using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ITemplateRepository
    {
        Task Incluir(Template template);
        // empresaIdSolicitante restringe a operacao aos templates da empresa informada.
        // null = administrador (sem restricao). Template tem EmpresaId proprio.
        Task<int> Alterar(Template template, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Template?> ObterPorId(int id);
        Task<IEnumerable<Template>> Obter();
        Task<IEnumerable<Template>> ObterPorEmpresa(Guid empresaId);
    }
}
