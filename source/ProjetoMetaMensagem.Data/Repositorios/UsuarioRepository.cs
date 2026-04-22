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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbSession _session;

        public UsuarioRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Usuario usuario)
        {
            var sql = $@"
                INSERT INTO {nameof(Usuario)} (
                    {nameof(Usuario.EmpresaId)}, 
                    {nameof(Usuario.Nome)}, 
                    {nameof(Usuario.Email)}, 
                    {nameof(Usuario.SenhaHash)}, 
                    {nameof(Usuario.DataCriacao)}
                ) 
                VALUES (
                    @EmpresaId, @Nome, @Email, @SenhaHash, @DataCriacao
                )";

            var parameters = new DynamicParameters();
            parameters.Add("Id", usuario.Id);
            parameters.Add("EmpresaId", usuario.EmpresaId);
            parameters.Add("Nome", usuario.Nome);
            parameters.Add("Email", usuario.Email);
            parameters.Add("SenhaHash", usuario.SenhaHash);
            parameters.Add("DataCriacao", DateTimeOffset.Now);

            await _session._connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
        }

        public async Task Alterar(Usuario usuario)
        {
            var sql = $@"
                UPDATE {nameof(Usuario)} 
                SET 
                    {nameof(Usuario.Nome)} = @Nome, 
                    {nameof(Usuario.Email)} = @Email, 
                    {nameof(Usuario.SenhaHash)} = @SenhaHash
                WHERE {nameof(Usuario.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, usuario, transaction: _session.Transaction);
        }

        public async Task<Usuario?> Logar(string email, string senhaHash)
        {
            var sql = $@"
             SELECT * FROM {nameof(Usuario)} 
             WHERE {nameof(Usuario.Email)} = @Email 
               AND {nameof(Usuario.SenhaHash)} = @SenhaHash";

            return await _session._connection.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new { Email = email, SenhaHash = senhaHash },
                transaction: _session.Transaction
            );
        }

        public async Task Excluir(string id)
        {
            var sql = $"DELETE FROM Usuarios WHERE {nameof(Usuario.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Usuario>> ObterPorEmpresa(Guid id)
        {
            var sql = $"SELECT * FROM Usuario WHERE {nameof(Usuario.Id)} = @Id";
            return await _session._connection.QueryAsync<Usuario>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Usuario>> Obter()
        {
            var sql = $"SELECT * FROM Usuario ORDER BY {nameof(Usuario.Nome)}";
            return await _session._connection.QueryAsync<Usuario>(sql, transaction: _session.Transaction);
        }

        public async Task<Usuario?> ObterPorId(Guid id)
        {
            var sql = $"SELECT * FROM Usuario WHERE {nameof(Usuario.Id)} = @Id";
            return await _session._connection.QueryFirstAsync<Usuario>(sql, transaction: _session.Transaction);
        }
    }
}
