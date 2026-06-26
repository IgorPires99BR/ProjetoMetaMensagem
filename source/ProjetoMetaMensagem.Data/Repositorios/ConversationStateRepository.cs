using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ConversationStateRepository : IConversationStateRepository
    {
        private readonly DbSession _session;

        public ConversationStateRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<ConversationState?> ObterPorEmpresaEContato(Guid empresaId, Guid contatoId)
        {
            var sql = $@"
                SELECT * FROM {nameof(ConversationState)}
                WHERE {nameof(ConversationState.EmpresaId)} = @EmpresaId
                  AND {nameof(ConversationState.ContatoId)} = @ContatoId
                  AND {nameof(ConversationState.Finalizado)} = 0;";

            return await _session.Connection.QueryFirstOrDefaultAsync<ConversationState>(
                sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task Incluir(ConversationState state)
        {
            var sql = $@"
                INSERT INTO {nameof(ConversationState)} (
                    {nameof(ConversationState.Id)}, {nameof(ConversationState.EmpresaId)}, {nameof(ConversationState.ContatoId)}, {nameof(ConversationState.FlowId)}, {nameof(ConversationState.EtapaAtualId)},
                    {nameof(ConversationState.Variaveis)}, {nameof(ConversationState.DataInicio)}, {nameof(ConversationState.DataAtualizacao)}, {nameof(ConversationState.Finalizado)}
                ) VALUES (
                    @Id, @EmpresaId, @ContatoId, @FlowId, @EtapaAtualId,
                    @Variaveis, @DataInicio, @DataAtualizacao, @Finalizado
                );";

            await _session.Connection.ExecuteAsync(sql, state, transaction: _session.Transaction);
        }

        public async Task Atualizar(ConversationState state)
        {
            var sql = $@"
                UPDATE {nameof(ConversationState)} SET
                    {nameof(ConversationState.EtapaAtualId)} = @EtapaAtualId,
                    {nameof(ConversationState.Variaveis)} = @Variaveis,
                    {nameof(ConversationState.DataAtualizacao)} = @DataAtualizacao,
                    {nameof(ConversationState.Finalizado)} = @Finalizado
                WHERE {nameof(ConversationState.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(sql, state, transaction: _session.Transaction);
        }

        public async Task<List<ConversationState>> ObterPorFlow(Guid flowId)
        {
            var sql = $@"
                SELECT * FROM {nameof(ConversationState)}
                WHERE {nameof(ConversationState.FlowId)} = @FlowId
                ORDER BY {nameof(ConversationState.DataAtualizacao)} DESC;";

            var result = await _session.Connection.QueryAsync<ConversationState>(
                sql, new { FlowId = flowId }, transaction: _session.Transaction);

            return result.ToList();
        }

        public async Task Excluir(Guid id)
        {
            var sql = $"DELETE FROM {nameof(ConversationState)} WHERE {nameof(ConversationState.Id)} = @Id;";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }
    }
}

