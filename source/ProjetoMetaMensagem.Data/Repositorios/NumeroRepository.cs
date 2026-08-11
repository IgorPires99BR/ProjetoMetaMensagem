using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class NumeroRepository : INumeroRepository
    {
        private readonly DbSession _session;

        public NumeroRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Numero numero)
        {
            var sql = $@"
                INSERT INTO {nameof(Numero)} (
                    {nameof(Numero.UsuarioId)},
                    {nameof(Numero.Telefone)},
                    {nameof(Numero.Descricao)},
                    {nameof(Numero.InstanciaId)},
                    {nameof(Numero.StatusMeta)},
                    {nameof(Numero.QualidadeMeta)},
                    {nameof(Numero.DataCriacao)},
                    {nameof(Numero.TipoConexao)},
                    {nameof(Numero.StatusConexao)},
                    {nameof(Numero.SystemUserToken)},
                    {nameof(Numero.WabaId)},
                    {nameof(Numero.DataUltimaSincronizacao)}
                )
                VALUES (@UsuarioId, @Telefone, @Descricao, @InstanciaId,@StatusMeta,@QualidadeMeta, @DataCriacao,
                    @TipoConexao, @StatusConexao, @SystemUserToken, @WabaId, @DataUltimaSincronizacao);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            await _session.Connection.ExecuteAsync(sql, numero, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar numero de outra empresa.
        // Numero nao guarda EmpresaId: o vinculo passa por Usuario.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL
                   OR UsuarioId IN (SELECT Id FROM Usuario WHERE EmpresaId = @EmpresaIdSolicitante))";

        public async Task<int> Alterar(Numero numero, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Numero)}
                SET
                    {nameof(Numero.Telefone)} = @Telefone,
                    {nameof(Numero.Descricao)} = @Descricao,
                    {nameof(Numero.InstanciaId)} = @InstanciaId,
                    {nameof(Numero.StatusMeta)} = @StatusMeta,
                    {nameof(Numero.QualidadeMeta)} = @QualidadeMeta,
                    {nameof(Numero.TipoConexao)} = @TipoConexao,
                    {nameof(Numero.StatusConexao)} = @StatusConexao,
                    {nameof(Numero.SystemUserToken)} = @SystemUserToken,
                    {nameof(Numero.WabaId)} = @WabaId,
                    {nameof(Numero.DataUltimaSincronizacao)} = @DataUltimaSincronizacao,
                    {nameof(Numero.DataAtualizacao)} = @DataAtualizacao
                WHERE {nameof(Numero.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    numero.Id,
                    numero.Telefone,
                    numero.Descricao,
                    numero.InstanciaId,
                    numero.StatusMeta,
                    numero.QualidadeMeta,
                    numero.TipoConexao,
                    numero.StatusConexao,
                    numero.SystemUserToken,
                    numero.WabaId,
                    numero.DataUltimaSincronizacao,
                    numero.DataAtualizacao,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Numero)}
                WHERE {nameof(Numero.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Numero?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM {nameof(Numero)} WHERE {nameof(Numero.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Numero>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Numero?> ObterPorId(Guid id)
        {
            var sql = $"SELECT * FROM {nameof(Numero)} WHERE {nameof(Numero.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Numero>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Numero?> ObterPorInstanciaId(string instanciaId)
        {
            var sql = $"SELECT * FROM {nameof(Numero)} WHERE {nameof(Numero.InstanciaId)} = @InstanciaId";
            return await _session.Connection.QueryFirstOrDefaultAsync<Numero>(sql, new { InstanciaId = instanciaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> Obter()
        {
            return await _session.Connection.QueryAsync<Numero>($"SELECT * FROM {nameof(Numero)}", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> ObterPorUsuario(Guid usuarioId)
        {
            var sql = $"SELECT * FROM {nameof(Numero)} WHERE {nameof(Numero.UsuarioId)} = @UsuarioId";
            return await _session.Connection.QueryAsync<Numero>(sql, new { UsuarioId = usuarioId }, transaction: _session.Transaction);
        }
    }
}

