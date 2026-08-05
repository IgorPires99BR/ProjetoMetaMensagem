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
        Task Excluir(Guid id);
    }
}
