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

            await _session.Connection.ExecuteAsync(sql, template, transaction: _session.Transaction);
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

            await _session.Connection.ExecuteAsync(sql, template, transaction: _session.Transaction);
        }

        public async Task Excluir(Guid id)
        {
            var sql = $"DELETE FROM {nameof(Template)} WHERE {nameof(Template.Id)} = @Id";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Template?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM {nameof(Template)} WHERE {nameof(Template.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Template>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> Obter()
        {
            return await _session.Connection.QueryAsync<Template>($"SELECT * FROM {nameof(Template)}", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> ObterPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(Template)} WHERE {nameof(Template.EmpresaId)} = @EmpresaId";
            return await _session.Connection.QueryAsync<Template>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }
    }
}

