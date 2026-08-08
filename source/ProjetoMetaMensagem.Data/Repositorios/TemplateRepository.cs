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
                    {nameof(Template.ComponentesJson)}, 
                    {nameof(Template.Idioma)}, 
                    {nameof(Template.Status)}
                ) 
                VALUES (@EmpresaId, @NomeTemplate, @Conteudo, @Categoria,@ComponentesJson, @Idioma, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            await _session.Connection.ExecuteAsync(sql, template, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar template de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Alterar(Template template, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Template)}
                SET
                    {nameof(Template.NomeTemplate)} = @NomeTemplate,
                    {nameof(Template.Conteudo)} = @Conteudo,
                    {nameof(Template.Categoria)} = @Categoria,
                    {nameof(Template.ComponentesJson)} = @ComponentesJson,
                    {nameof(Template.Status)} = @Status
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    template.Id,
                    template.NomeTemplate,
                    template.Conteudo,
                    template.Categoria,
                    template.ComponentesJson,
                    template.Status,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Template)}
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
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

