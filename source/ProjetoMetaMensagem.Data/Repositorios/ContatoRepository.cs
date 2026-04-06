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
    public class ContatoRepository : IContatoRepository
    {
        private readonly DbSession _session;

        public ContatoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Contato contato)
        {
            var sql = $@"
                INSERT INTO {nameof(Contato)} (
                    {nameof(Contato.UsuarioId)}, 
                    {nameof(Contato.Telefone)}, 
                    {nameof(Contato.Nome)}, 
                    {nameof(Contato.Email)}, 
                    {nameof(Contato.DataCriacao)}
                ) 
                VALUES (
                    @UsuarioId, @Telefone, @Nome, @Email, @DataCriacao
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var parameters = new
            {
                contato.UsuarioId,
                contato.Telefone,
                contato.Nome,
                contato.Email,
                DataCriacao = DateTimeOffset.Now
            };

            contato.Id = await _session._connection.QuerySingleAsync<int>(sql, parameters, transaction: _session.Transaction);
        }

        public async Task Alterar(Contato contato)
        {
            var sql = $@"
                UPDATE {nameof(Contato)} 
                SET 
                    {nameof(Contato.Telefone)} = @Telefone, 
                    {nameof(Contato.Nome)} = @Nome, 
                    {nameof(Contato.Email)} = @Email
                WHERE {nameof(Contato.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, contato, transaction: _session.Transaction);
        }

        public async Task Excluir(int id)
        {
            var sql = $"DELETE FROM Contatos WHERE {nameof(Contato.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Contato?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM Contatos WHERE {nameof(Contato.Id)} = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Contato>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Contato>> Obter()
        {
            var sql = $"SELECT * FROM Contatos";
            return await _session._connection.QueryAsync<Contato>(sql, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Contato>> ObterPorUsuario(string usuarioId)
        {
            var sql = $"SELECT * FROM Contatos WHERE {nameof(Contato.UsuarioId)} = @UsuarioId";
            return await _session._connection.QueryAsync<Contato>(sql, new { UsuarioId = usuarioId }, transaction: _session.Transaction);
        }
    }
}
