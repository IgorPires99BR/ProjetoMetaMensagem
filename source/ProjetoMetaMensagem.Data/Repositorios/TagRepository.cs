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

            await _session.Connection.ExecuteAsync(sql, tag, transaction: _session.Transaction);
            return tag.Id;
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o DELETE casava so pelo Id, entao
        // bastava conhecer (ou adivinhar) o id pra apagar tag de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            // ContatoTag nao guarda EmpresaId: o vinculo passa pela Tag. Sem esse recorte,
            // os vinculos de uma tag alheia seriam limpos mesmo com o DELETE da Tag barrado.
            var sqlVinculos = $@"
                DELETE FROM {nameof(ContatoTag)}
                WHERE {nameof(ContatoTag.TagId)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(ContatoTag.TagId)} IN (
                           SELECT {nameof(Tag.Id)} FROM {nameof(Tag)}
                           WHERE {nameof(Tag.EmpresaId)} = @EmpresaIdSolicitante));";

            await _session.Connection.ExecuteAsync(sqlVinculos,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            // So o DELETE da propria tag conta como exclusao; os vinculos removidos acima
            // inflariam o total e mascarariam uma tag inexistente.
            var sql = $@"
                DELETE FROM {nameof(Tag)}
                WHERE {nameof(Tag.Id)} = @Id
                {RecorteDaEmpresa};";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Tag>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Tag)}
                WHERE {nameof(Tag.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(Tag.Nome)};";

            return await _session.Connection.QueryAsync<Tag>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<Tag?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Tag)}
                WHERE {nameof(Tag.Id)} = @Id;";

            return await _session.Connection.QueryFirstOrDefaultAsync<Tag>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Tag>> ObterPorContato(Guid contatoId)
        {
            var sql = $@"
                SELECT t.* FROM {nameof(Tag)} t
                INNER JOIN {nameof(ContatoTag)} ct ON t.{nameof(Tag.Id)} = ct.{nameof(ContatoTag.TagId)}
                WHERE ct.{nameof(ContatoTag.ContatoId)} = @ContatoId
                ORDER BY t.{nameof(Tag.Nome)};";

            return await _session.Connection.QueryAsync<Tag>(
                sql, new { ContatoId = contatoId }, transaction: _session.Transaction);
        }

        // Contato chega na empresa por Usuario; Tag tem EmpresaId proprio. Sem esses recortes
        // qualquer usuario logado reescrevia as tags de um contato de outra empresa (o DELETE
        // abaixo limpa todos os vinculos do contato) so mandando o ContatoId no corpo.
        private const string ContatoDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL
                   OR EXISTS (SELECT 1 FROM Contato c
                              INNER JOIN Usuario u ON u.Id = c.UsuarioId
                              WHERE c.Id = @ContatoId AND u.EmpresaId = @EmpresaIdSolicitante))";

        public async Task AssociarTagsContato(Guid contatoId, List<Guid> tagIds, Guid? empresaIdSolicitante)
        {
            // Remove associações existentes
            var sqlDelete = $@"
                DELETE FROM {nameof(ContatoTag)}
                WHERE {nameof(ContatoTag.ContatoId)} = @ContatoId
                {ContatoDaEmpresa};";

            await _session.Connection.ExecuteAsync(sqlDelete,
                new { ContatoId = contatoId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            if (tagIds == null || tagIds.Count == 0) return;

            // Insere novas associações. O INSERT ... SELECT ... WHERE deixa o recorte valer
            // tambem na inclusao: nem contato de outra empresa, nem tag de outra empresa.
            var sqlInsert = $@"
                INSERT INTO {nameof(ContatoTag)} (
                    {nameof(ContatoTag.ContatoId)},
                    {nameof(ContatoTag.TagId)},
                    {nameof(ContatoTag.DataCriacao)}
                )
                SELECT @ContatoId, @TagId, @DataCriacao
                WHERE 1 = 1
                {ContatoDaEmpresa}
                  AND (@EmpresaIdSolicitante IS NULL
                       OR EXISTS (SELECT 1 FROM {nameof(Tag)}
                                  WHERE {nameof(Tag.Id)} = @TagId
                                    AND {nameof(Tag.EmpresaId)} = @EmpresaIdSolicitante));";

            foreach (var tagId in tagIds)
            {
                await _session.Connection.ExecuteAsync(sqlInsert,
                    new
                    {
                        ContatoId = contatoId,
                        TagId = tagId,
                        DataCriacao = DateTime.Now,
                        EmpresaIdSolicitante = empresaIdSolicitante
                    },
                    transaction: _session.Transaction);
            }
        }
    }
}

