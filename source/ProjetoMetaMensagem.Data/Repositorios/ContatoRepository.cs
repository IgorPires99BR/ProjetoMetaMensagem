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

            await _session.Connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
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

            await _session.Connection.ExecuteAsync(sql, contato, transaction: _session.Transaction);
        }

        public async Task Excluir(string id)
        {
            var sql = $"DELETE FROM {nameof(Contato)} WHERE {nameof(Contato.Id)} = @Id";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Contato?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM {nameof(Contato)} WHERE {nameof(Contato.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Contato>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Contato>> Obter()
        {
            var sql = $"SELECT * FROM {nameof(Contato)}";
            return await _session.Connection.QueryAsync<Contato>(sql, transaction: _session.Transaction);
        }

        public async Task<Contato?> ObterPorTelefone(Guid empresaId, string telefone)
        {
            // Contato nao tem EmpresaId direto (so UsuarioId), entao o escopo por empresa
            // precisa passar por Usuario. Sem esse JOIN/WHERE (como estava antes), a busca
            // batia com QUALQUER contato de QUALQUER empresa que tivesse o mesmo telefone --
            // um vazamento entre tenants, e a causa de mensagens do webhook nao encontrarem
            // o contato certo (ou nenhum) quando havia telefone duplicado entre usuarios.
            // Tambem normaliza os dois lados pra digitos apenas, ja que a Meta manda o "from"
            // sempre sem "+"/espacos, mas o cadastro manual do Contato pode ter formatacao.
            var sql = @"
        SELECT c.* FROM Contato c
        INNER JOIN Usuario u ON u.Id = c.UsuarioId
        WHERE u.EmpresaId = @EmpresaId
          AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(c.Telefone, '+', ''), ' ', ''), '-', ''), '(', ''), ')', '') = @Telefone";

            var telefoneNormalizado = new string(telefone.Where(char.IsDigit).ToArray());

            return await _session.Connection.QueryFirstOrDefaultAsync<Contato>(
                sql,
                new { EmpresaId = empresaId, Telefone = telefoneNormalizado },
                transaction: _session.Transaction
            );
        }


        public async Task<IEnumerable<Contato>> ObterPorUsuario(Guid usuarioId)
        {
            var sql = $"SELECT * FROM {nameof(Contato)} WHERE {nameof(Contato.UsuarioId)} = @UsuarioId";
            return await _session.Connection.QueryAsync<Contato>(sql, new { UsuarioId = usuarioId }, transaction: _session.Transaction);
        }
    }
}

