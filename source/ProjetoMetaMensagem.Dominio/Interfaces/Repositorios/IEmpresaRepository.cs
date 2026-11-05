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
        // Mais de uma quando dividem o numero (ex: Sebrecon no da Contact Solution). A mais antiga
        // vem primeiro: e a dona original do numero.
        Task<List<Guid>> ObterTodasPorPhoneNumberId(string phoneNumberId);

        Task AtualizarWabaId(Guid id, string wabaId);
        // Usado pelo Embedded Signup a nivel de Empresa (ConectaContaMetaHandler): grava
        // WabaId/PhoneNumberId/MetaAccessToken de uma vez, sem exigir os demais campos da
        // empresa (diferente de Alterar, que sobrescreve tudo).
        Task AtualizarCredenciaisMeta(Guid id, string? wabaId, string? phoneNumberId, string? metaAccessToken);
        Task<List<Empresa>> Obter();
    }
}
