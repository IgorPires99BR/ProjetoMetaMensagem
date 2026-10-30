using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class DadosBancariosEmpresaRepository : IDadosBancariosEmpresaRepository
    {
        private readonly DbSession _session;

        public DadosBancariosEmpresaRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<DadosBancariosEmpresa?> ObterPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(DadosBancariosEmpresa)} WHERE {nameof(DadosBancariosEmpresa.EmpresaId)} = @EmpresaId";
            return await _session.Connection.QueryFirstOrDefaultAsync<DadosBancariosEmpresa>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        // As colunas antigas do Itau e da Transfeera (Banco, Ambiente, ChavePix etc.) ficam de
        // fora: no INSERT recebem o DEFAULT da tabela, no UPDATE continuam como estavam.
        public async Task Salvar(DadosBancariosEmpresa dados)
        {
            var sql = @"
                UPDATE DadosBancariosEmpresa
                SET AsaasContaId = @AsaasContaId,
                    AsaasWalletId = @AsaasWalletId,
                    AsaasApiKeyCriptografada = @AsaasApiKeyCriptografada,
                    AsaasContaCriadaEm = @AsaasContaCriadaEm,
                    ComissaoTipo = @ComissaoTipo,
                    ComissaoValor = @ComissaoValor,
                    RecebimentoNaPlataforma = @RecebimentoNaPlataforma,
                    CobrancaPixAtiva = @CobrancaPixAtiva,
                    DataAtualizacao = GETDATE()
                WHERE EmpresaId = @EmpresaId;

                IF @@ROWCOUNT = 0
                    INSERT INTO DadosBancariosEmpresa (
                        EmpresaId, AsaasContaId, AsaasWalletId, AsaasApiKeyCriptografada, AsaasContaCriadaEm,
                        ComissaoTipo, ComissaoValor, RecebimentoNaPlataforma, CobrancaPixAtiva, DataCriacao)
                    VALUES (
                        @EmpresaId, @AsaasContaId, @AsaasWalletId, @AsaasApiKeyCriptografada, @AsaasContaCriadaEm,
                        @ComissaoTipo, @ComissaoValor, @RecebimentoNaPlataforma, @CobrancaPixAtiva, GETDATE());";

            await _session.Connection.ExecuteAsync(sql, dados, transaction: _session.Transaction);
        }
    }
}
