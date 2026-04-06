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
    public class TemplateRepository : ITemplateRepository
    {
        private readonly DbSession _session;

        public TemplateRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Template template)
        {
            var sql = $@"
                INSERT INTO {nameof(Template)} (
                    {nameof(Template.EmpresaId)}, 
                    {nameof(Template.NomeTemplate)}, 
                    {nameof(Template.Conteudo)}, 
                    {nameof(Template.Categoria)}, 
                    {nameof(Template.Idioma)}, 
                    {nameof(Template.Status)}
                ) 
                VALUES (@EmpresaId, @NomeTemplate, @Conteudo, @Categoria, @Idioma, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            template.Id = await _session._connection.QuerySingleAsync<int>(sql, template, transaction: _session.Transaction);
        }

        public async Task Alterar(Template template)
        {
            var sql = $@"
                UPDATE {nameof(Template)} 
                SET 
                    {nameof(Template.NomeTemplate)} = @NomeTemplate, 
                    {nameof(Template.Conteudo)} = @Conteudo, 
                    {nameof(Template.Categoria)} = @Categoria, 
                    {nameof(Template.Status)} = @Status
                WHERE {nameof(Template.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, template, transaction: _session.Transaction);
        }

        public async Task Excluir(int id)
        {
            var sql = $"DELETE FROM Templates WHERE {nameof(Template.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Template?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM Templates WHERE {nameof(Template.Id)} = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Template>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> Obter()
        {
            return await _session._connection.QueryAsync<Template>("SELECT * FROM Templates", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> ObterPorEmpresa(string empresaId)
        {
            var sql = $"SELECT * FROM Templates WHERE {nameof(Template.EmpresaId)} = @EmpresaId";
            return await _session._connection.QueryAsync<Template>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }
    }
}
