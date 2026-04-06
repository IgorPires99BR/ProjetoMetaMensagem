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
        Task Alterar(Template template);
        Task Excluir(int id);
        Task<Template?> ObterPorId(int id);
        Task<IEnumerable<Template>> Obter();
        Task<IEnumerable<Template>> ObterPorEmpresa(string empresaId);
    }
}
