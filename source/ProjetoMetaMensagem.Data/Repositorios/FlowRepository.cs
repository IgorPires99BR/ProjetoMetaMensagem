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

        public async Task<FlowEtapa?> ObterEtapaInicial(Guid flowId)
        {
            var sql = $@"
                SELECT * FROM {nameof(FlowEtapa)} 
                WHERE {nameof(FlowEtapa.FlowId)} = @{nameof(FlowEtapa.FlowId)} 
                  AND {nameof(FlowEtapa.EhEtapaInicial)} = 1;";

            return await _session._connection.QueryFirstOrDefaultAsync<FlowEtapa>(
                sql, new { FlowId = flowId }, transaction: _session.Transaction);
        }

        public async Task<FlowEtapa?> ObterProximaEtapa(Guid etapaAtualId, string respostaCliente)
        {
            var sql = $@"
                SELECT proxima.* FROM {nameof(FlowEtapa)} atual
                INNER JOIN {nameof(FlowEtapa)} proxima ON atual.{nameof(FlowEtapa.ProximaEtapaId)} = proxima.{nameof(FlowEtapa.Id)}
                WHERE atual.{nameof(FlowEtapa.Id)} = @EtapaAtualId 
                  AND (atual.{nameof(FlowEtapa.GatilhoResposta)} = @RespostaCliente OR atual.{nameof(FlowEtapa.GatilhoResposta)} = 'Qualquer_Resposta');";

            return await _session._connection.QueryFirstOrDefaultAsync<FlowEtapa>(
                sql, new { EtapaAtualId = etapaAtualId, RespostaCliente = respostaCliente?.Trim() }, transaction: _session.Transaction);
        }

        #endregion

        #region CRUD Base (Flow)

        public async Task<Flow?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Flow)} 
                WHERE {nameof(Flow.Id)} = @{nameof(Flow.Id)};";

            return await _session._connection.QueryFirstOrDefaultAsync<Flow>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Flow>> ObterTodosPorEmpresa(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Flow)} 
                WHERE {nameof(Flow.EmpresaId)} = @{nameof(Flow.EmpresaId)}
                ORDER BY {nameof(Flow.DataCriacao)} DESC;";

            return await _session._connection.QueryAsync<Flow>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task Incluir(Flow flow)
        {
            var sql = $@"
                INSERT INTO {nameof(Flow)} (
                    {nameof(flow.Id)}, 
                    {nameof(flow.EmpresaId)}, 
                    {nameof(flow.Nome)}, 
                    {nameof(flow.Descricao)}, 
                    {nameof(flow.Ativo)}, 
                    {nameof(flow.DataCriacao)}
                ) 
                VALUES (
                    @{nameof(flow.Id)}, 
                    @{nameof(flow.EmpresaId)}, 
                    @{nameof(flow.Nome)}, 
                    @{nameof(flow.Descricao)}, 
                    @{nameof(flow.Ativo)}, 
                    @{nameof(flow.DataCriacao)}
                );";

            await _session._connection.ExecuteAsync(sql, flow, transaction: _session.Transaction);
        }

        public async Task Alterar(Flow flow)
        {
            var sql = $@"
                UPDATE {nameof(Flow)} SET 
                    {nameof(flow.Nome)} = @{nameof(flow.Nome)}, 
                    {nameof(flow.Descricao)} = @{nameof(flow.Descricao)}, 
                    {nameof(flow.Ativo)} = @{nameof(flow.Ativo)}
                WHERE {nameof(flow.Id)} = @{nameof(flow.Id)};";

            await _session._connection.ExecuteAsync(sql, flow, transaction: _session.Transaction);
        }

        public async Task Excluir(Guid id)
        {
            var sql = $@"
                DELETE FROM {nameof(Flow)} 
                WHERE {nameof(Flow.Id)} = @Id;";

            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        #endregion
    }
}
