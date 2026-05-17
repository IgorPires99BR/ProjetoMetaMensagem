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
                    {nameof(Numero.DataCriacao)}
                ) 
                VALUES (@UsuarioId, @Telefone, @Descricao, @InstanciaId,@StatusMeta,@QualidadeMeta, @DataCriacao);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            await _session._connection.QuerySingleAsync<Numero>(sql, numero, transaction: _session.Transaction);
        }

        public async Task Alterar(Numero numero)
        {
            var sql = $@"
                UPDATE {nameof(Numero)} 
                SET 
                    {nameof(Numero.Telefone)} = @Telefone, 
                    {nameof(Numero.Descricao)} = @Descricao, 
                    {nameof(Numero.InstanciaId)} = @InstanciaId,
                    {nameof(Numero.StatusMeta)} = @StatusMeta,
                    {nameof(Numero.QualidadeMeta)} = @QualidadeMeta
                WHERE {nameof(Numero.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, numero, transaction: _session.Transaction);
        }

        public async Task Excluir(Guid id)
        {
            var sql = $"DELETE FROM Numero WHERE {nameof(Numero.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Numero?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM Numero WHERE {nameof(Numero.Id)} = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Numero>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> Obter()
        {
            return await _session._connection.QueryAsync<Numero>($"SELECT * FROM Numero", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> ObterPorUsuario(Guid usuarioId)
        {
            var sql = $"SELECT * FROM Numero WHERE {nameof(Numero.UsuarioId)} = @UsuarioId";
            return await _session._connection.QueryAsync<Numero>(sql, new { UsuarioId = usuarioId }, transaction: _session.Transaction);
        }
    }
}
