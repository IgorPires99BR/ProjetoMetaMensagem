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
        Task<int> Excluir(string id);
        Task<Empresa?> ObterPorId(Guid id);
        Task<string?> ObterMetaAccessToken(Guid id);

        Task<string?> ObterWabaId(Guid id);
        Task<string?> ObterPhoneNumberId(Guid id);
        Task<string?> ObterAppIdMeta(Guid id);
        Task<Guid?> ObterPorPhoneNumberId(string phoneNumberId);

        Task AtualizarWabaId(Guid id, string wabaId);
        // Usado pelo Embedded Signup a nivel de Empresa (ConectaContaMetaHandler): grava
        // WabaId/PhoneNumberId/MetaAccessToken de uma vez, sem exigir os demais campos da
        // empresa (diferente de Alterar, que sobrescreve tudo).
        Task AtualizarCredenciaisMeta(Guid id, string? wabaId, string? phoneNumberId, string? metaAccessToken);
        Task<List<Empresa>> Obter();
    }
}
