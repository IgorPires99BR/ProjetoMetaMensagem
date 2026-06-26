using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class TagRepository : ITagRepository
    {
        private readonly DbSession _session;

        public TagRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<Guid> Incluir(Tag tag)
        {
            var sql = $@"
                INSERT INTO {nameof(Tag)} (
                    {nameof(Tag.Id)},
                    {nameof(Tag.EmpresaId)},
                    {nameof(Tag.Nome)},
                    {nameof(Tag.Cor)},
                    {nameof(Tag.DataCriacao)}
                ) VALUES (
                    @{nameof(Tag.Id)},
                    @{nameof(Tag.EmpresaId)},
                    @{nameof(Tag.Nome)},
                    @{nameof(Tag.Cor)},
                    @{nameof(Tag.DataCriacao)}
                );";

            await _session._connection.ExecuteAsync(sql, tag, transaction: _session.Transaction);
            return tag.Id;
        }

        public async Task Excluir(Guid id)
        {
            var sql = $@"
                DELETE FROM {nameof(ContatoTag)} WHERE {nameof(ContatoTag.TagId)} = @Id;
                DELETE FROM {nameof(Tag)} WHERE {nameof(Tag.Id)} = @Id;";

            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Tag>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Tag)}
                WHERE {nameof(Tag.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(Tag.Nome)};";

            return await _session._connection.QueryAsync<Tag>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<Tag?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Tag)}
                WHERE {nameof(Tag.Id)} = @Id;";

            return await _session._connection.QueryFirstOrDefaultAsync<Tag>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Tag>> ObterPorContato(Guid contatoId)
        {
            var sql = $@"
                SELECT t.* FROM {nameof(Tag)} t
                INNER JOIN {nameof(ContatoTag)} ct ON t.{nameof(Tag.Id)} = ct.{nameof(ContatoTag.TagId)}
                WHERE ct.{nameof(ContatoTag.ContatoId)} = @ContatoId
                ORDER BY t.{nameof(Tag.Nome)};";

            return await _session._connection.QueryAsync<Tag>(
                sql, new { ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task AssociarTagsContato(Guid contatoId, List<Guid> tagIds)
        {
            // Remove associações existentes
            var sqlDelete = $@"
                DELETE FROM {nameof(ContatoTag)}
                WHERE {nameof(ContatoTag.ContatoId)} = @ContatoId;";

            await _session._connection.ExecuteAsync(sqlDelete,
                new { ContatoId = contatoId }, transaction: _session.Transaction);

            if (tagIds == null || tagIds.Count == 0) return;

            // Insere novas associações
            var sqlInsert = $@"
                INSERT INTO {nameof(ContatoTag)} (
                    {nameof(ContatoTag.ContatoId)},
                    {nameof(ContatoTag.TagId)},
                    {nameof(ContatoTag.DataCriacao)}
                ) VALUES (
                    @ContatoId,
                    @TagId,
                    @DataCriacao
                );";

            foreach (var tagId in tagIds)
            {
                await _session._connection.ExecuteAsync(sqlInsert,
                    new { ContatoId = contatoId, TagId = tagId, DataCriacao = DateTime.Now },
                    transaction: _session.Transaction);
            }
        }
    }
}
