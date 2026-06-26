using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class WebhookConfigRepository : IWebhookConfigRepository
    {
        private readonly DbSession _session;

        public WebhookConfigRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<Guid> Incluir(WebhookConfig webhookConfig)
        {
            var sql = $@"
                INSERT INTO {nameof(WebhookConfig)} (
                    {nameof(WebhookConfig.Id)},
                    {nameof(WebhookConfig.EmpresaId)},
                    {nameof(WebhookConfig.Nome)},
                    {nameof(WebhookConfig.Url)},
                    {nameof(WebhookConfig.Evento)},
                    {nameof(WebhookConfig.TokenSegredo)},
                    {nameof(WebhookConfig.Ativo)},
                    {nameof(WebhookConfig.DataCriacao)}
                ) VALUES (
                    @{nameof(WebhookConfig.Id)},
                    @{nameof(WebhookConfig.EmpresaId)},
                    @{nameof(WebhookConfig.Nome)},
                    @{nameof(WebhookConfig.Url)},
                    @{nameof(WebhookConfig.Evento)},
                    @{nameof(WebhookConfig.TokenSegredo)},
                    @{nameof(WebhookConfig.Ativo)},
                    @{nameof(WebhookConfig.DataCriacao)}
                );";

            await _session.Connection.ExecuteAsync(sql, webhookConfig, transaction: _session.Transaction);
            return webhookConfig.Id;
        }

        public async Task<Guid> Alterar(WebhookConfig webhookConfig)
        {
            var sql = $@"
                UPDATE {nameof(WebhookConfig)} SET
                    {nameof(WebhookConfig.Nome)} = @{nameof(WebhookConfig.Nome)},
                    {nameof(WebhookConfig.Url)} = @{nameof(WebhookConfig.Url)},
                    {nameof(WebhookConfig.Evento)} = @{nameof(WebhookConfig.Evento)},
                    {nameof(WebhookConfig.TokenSegredo)} = @{nameof(WebhookConfig.TokenSegredo)},
                    {nameof(WebhookConfig.Ativo)} = @{nameof(WebhookConfig.Ativo)}
                WHERE {nameof(WebhookConfig.Id)} = @{nameof(WebhookConfig.Id)};";

            await _session.Connection.ExecuteAsync(sql, webhookConfig, transaction: _session.Transaction);
            return webhookConfig.Id;
        }

        public async Task Excluir(Guid id)
        {
            var sql = $@"DELETE FROM {nameof(WebhookConfig)} WHERE {nameof(WebhookConfig.Id)} = @Id;";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<WebhookConfig?> ObterPorId(Guid id)
        {
            var sql = $@"SELECT * FROM {nameof(WebhookConfig)} WHERE {nameof(WebhookConfig.Id)} = @Id;";
            return await _session.Connection.QueryFirstOrDefaultAsync<WebhookConfig>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<WebhookConfig>> ObterPorEmpresa(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(WebhookConfig)}
                WHERE {nameof(WebhookConfig.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(WebhookConfig.Nome)};";

            return (await _session.Connection.QueryAsync<WebhookConfig>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction)).ToList();
        }

        public async Task<List<WebhookConfig>> ObterAtivosPorEvento(string evento, Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(WebhookConfig)}
                WHERE {nameof(WebhookConfig.Evento)} = @Evento
                  AND {nameof(WebhookConfig.EmpresaId)} = @EmpresaId
                  AND {nameof(WebhookConfig.Ativo)} = 1;";

            return (await _session.Connection.QueryAsync<WebhookConfig>(
                sql, new { Evento = evento, EmpresaId = empresaId }, transaction: _session.Transaction)).ToList();
        }

        public async Task<List<WebhookConfig>> Obter()
        {
            var sql = $@"SELECT * FROM {nameof(WebhookConfig)} ORDER BY {nameof(WebhookConfig.Nome)};";
            return (await _session.Connection.QueryAsync<WebhookConfig>(sql, transaction: _session.Transaction)).ToList();
        }
    }
}

