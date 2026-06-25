using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IEmpresaRepository
    {
        Task<Guid> Incluir(Empresa empresa);
        Task<Guid> Alterar(Empresa empresa);
        Task Excluir(string id);
        Task<Empresa?> ObterPorId(Guid id);
        Task<string?> ObterMetaAccessToken(Guid id);

        Task<string?> ObterWabaId(Guid id);

        Task AtualizarWabaId(Guid id, string wabaId);
        Task<List<Empresa>> Obter();
    }
}
