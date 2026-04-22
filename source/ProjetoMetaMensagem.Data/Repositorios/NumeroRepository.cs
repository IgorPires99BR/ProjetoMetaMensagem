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
                    {nameof(Numero.NumeroTelefone)}, 
                    {nameof(Numero.Descricao)}, 
                    {nameof(Numero.InstanciaId)}
                ) 
                VALUES (@UsuarioId, @NumeroTelefone, @Descricao, @InstanciaId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            await _session._connection.QuerySingleAsync<int>(sql, numero, transaction: _session.Transaction);
        }

        public async Task Alterar(Numero numero)
        {
            var sql = $@"
                UPDATE {nameof(Numero)} 
                SET 
                    {nameof(Numero.NumeroTelefone)} = @NumeroTelefone, 
                    {nameof(Numero.Descricao)} = @Descricao, 
                    {nameof(Numero.InstanciaId)} = @InstanciaId
                WHERE {nameof(Numero.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, numero, transaction: _session.Transaction);
        }

        public async Task Excluir(string id)
        {
            var sql = $"DELETE FROM Numeros WHERE {nameof(Numero.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Numero?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM Numeros WHERE {nameof(Numero.Id)} = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Numero>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> Obter()
        {
            return await _session._connection.QueryAsync<Numero>($"SELECT * FROM Numeros", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Numero>> ObterPorUsuario(string usuarioId)
        {
            var sql = $"SELECT * FROM Numeros WHERE {nameof(Numero.UsuarioId)} = @UsuarioId";
            return await _session._connection.QueryAsync<Numero>(sql, new { UsuarioId = usuarioId }, transaction: _session.Transaction);
        }
    }
}
