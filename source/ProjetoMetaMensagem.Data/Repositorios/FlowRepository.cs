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
    public class FlowRepository : IFlowRepository
    {
        private readonly DbSession _session;

        public FlowRepository(DbSession session)
        {
            _session = session;
        }

        #region Consultas de Automação (Chatbot)

        public async Task<FlowEtapa?> ObterEtapaPorId(Guid etapaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(FlowEtapa)}
                WHERE {nameof(FlowEtapa.Id)} = @Id;";

            return await _session.Connection.QueryFirstOrDefaultAsync<FlowEtapa>(
                sql, new { Id = etapaId }, transaction: _session.Transaction);
        }

        public async Task<FlowEtapa?> ObterEtapaInicial(Guid flowId)
        {
            var sql = $@"
                SELECT * FROM {nameof(FlowEtapa)} 
                WHERE {nameof(FlowEtapa.FlowId)} = @{nameof(FlowEtapa.FlowId)} 
                  AND {nameof(FlowEtapa.EhEtapaInicial)} = 1;";

            return await _session.Connection.QueryFirstOrDefaultAsync<FlowEtapa>(
                sql, new { FlowId = flowId }, transaction: _session.Transaction);
        }

        public async Task<FlowEtapa?> ObterProximaEtapa(Guid etapaAtualId, string respostaCliente)
        {
            var sql = $@"
                SELECT proxima.* FROM {nameof(FlowEtapa)} atual
                INNER JOIN {nameof(FlowEtapa)} proxima ON atual.{nameof(FlowEtapa.ProximaEtapaId)} = proxima.{nameof(FlowEtapa.Id)}
                WHERE atual.{nameof(FlowEtapa.Id)} = @EtapaAtualId 
                  AND (atual.{nameof(FlowEtapa.GatilhoResposta)} = @RespostaCliente OR atual.{nameof(FlowEtapa.GatilhoResposta)} = 'Qualquer_Resposta');";

            return await _session.Connection.QueryFirstOrDefaultAsync<FlowEtapa>(
                sql, new { EtapaAtualId = etapaAtualId, RespostaCliente = respostaCliente?.Trim() }, transaction: _session.Transaction);
        }

        #endregion

        #region CRUD Base (Flow)

        public async Task<Flow?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Flow)} 
                WHERE {nameof(Flow.Id)} = @{nameof(Flow.Id)};";

            return await _session.Connection.QueryFirstOrDefaultAsync<Flow>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Flow>> ObterTodosPorEmpresa(Guid empresaId)
        {
            var sql = @"
                SELECT f.*, fe.*
                FROM Flow f
                LEFT JOIN FlowEtapa fe ON f.Id = fe.FlowId
                WHERE f.EmpresaId = @IdEmpresa;";

            var lookup = new Dictionary<Guid, Flow>();

            await _session.Connection.QueryAsync<Flow, FlowEtapa, Flow>(
                sql,
                (flow, etapa) =>
                {
                    if (!lookup.TryGetValue(flow.Id, out var flowExistente))
                    {
                        flowExistente = flow;
                        flowExistente.Etapas = new List<FlowEtapa>();
                        lookup.Add(flowExistente.Id, flowExistente);
                    }

                    if (etapa != null)
                    {
                        flowExistente.Etapas.Add(etapa);
                    }

                    return flowExistente;
                },
                new { IdEmpresa = empresaId },
                transaction: _session.Transaction,
                splitOn: "Id" // Divide as tabelas a partir da coluna Id da FlowEtapa
            );

            return lookup.Values;
        }

        public async Task Incluir(Flow flow)
        {
            var sql = $@"
                INSERT INTO {nameof(Flow)} (
                    {nameof(flow.Id)}, 
                    {nameof(flow.EmpresaId)}, 
                    {nameof(flow.Nome)}, 
                    {nameof(flow.Descricao)},
                    {nameof(flow.GatilhoInicial)},
                    {nameof(flow.Ativo)},
                    {nameof(flow.DataCriacao)}
                )
                VALUES (
                    @{nameof(flow.Id)},
                    @{nameof(flow.EmpresaId)},
                    @{nameof(flow.Nome)},
                    @{nameof(flow.Descricao)},
                    @{nameof(flow.GatilhoInicial)},
                    @{nameof(flow.Ativo)},
                    @{nameof(flow.DataCriacao)}
                );";

            await _session.Connection.ExecuteAsync(sql, flow, transaction: _session.Transaction);
        }

        public async Task IncluirEtapa(FlowEtapa etapa)
        {
            var sql = $@"
        INSERT INTO {nameof(FlowEtapa)} (
            {nameof(etapa.Id)}, 
            {nameof(etapa.FlowId)}, 
            {nameof(etapa.TemplateId)}, 
            {nameof(etapa.NomeEtapa)}, 
            {nameof(etapa.ConteudoLivre)}, 
            {nameof(etapa.GatilhoResposta)}, 
            {nameof(etapa.ProximaEtapaId)}, 
            {nameof(etapa.EhEtapaInicial)}
        ) 
        VALUES (
            @{nameof(etapa.Id)}, 
            @{nameof(etapa.FlowId)}, 
            @{nameof(etapa.TemplateId)}, 
            @{nameof(etapa.NomeEtapa)}, 
            @{nameof(etapa.ConteudoLivre)}, 
            @{nameof(etapa.GatilhoResposta)}, 
            @{nameof(etapa.ProximaEtapaId)}, 
            @{nameof(etapa.EhEtapaInicial)}
        );";

            await _session.Connection.ExecuteAsync(sql, etapa, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar fluxo de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> ExcluirEtapasPorFlowId(Guid flowId, Guid? empresaIdSolicitante)
        {
            // FlowEtapa nao guarda EmpresaId: o vinculo passa pelo Flow. Sem esse recorte,
            // a exclusao em cascata do DeletaFlow limpava as etapas de um fluxo alheio
            // mesmo quando o DELETE do proprio Flow era barrado.
            var sql = $@"
                DELETE FROM {nameof(FlowEtapa)}
                WHERE {nameof(FlowEtapa.FlowId)} = @FlowId
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(FlowEtapa.FlowId)} IN (
                           SELECT {nameof(Flow.Id)} FROM {nameof(Flow)}
                           WHERE {nameof(Flow.EmpresaId)} = @EmpresaIdSolicitante));";

            return await _session.Connection.ExecuteAsync(sql,
                new { FlowId = flowId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<int> Alterar(Flow flow, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Flow)} SET
                    {nameof(flow.Nome)} = @{nameof(flow.Nome)},
                    {nameof(flow.Descricao)} = @{nameof(flow.Descricao)},
                    {nameof(flow.GatilhoInicial)} = @{nameof(flow.GatilhoInicial)},
                    {nameof(flow.Ativo)} = @{nameof(flow.Ativo)}
                WHERE {nameof(flow.Id)} = @{nameof(flow.Id)}
                {RecorteDaEmpresa};";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    flow.Id,
                    flow.Nome,
                    flow.Descricao,
                    flow.GatilhoInicial,
                    flow.Ativo,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Flow)}
                WHERE {nameof(Flow.Id)} = @Id
                {RecorteDaEmpresa};";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        #endregion
    }
}

