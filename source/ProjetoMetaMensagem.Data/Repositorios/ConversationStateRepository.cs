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
            var sql = @"
                SELECT * FROM ConversationState
                WHERE EmpresaId = @EmpresaId
                  AND ContatoId = @ContatoId
                  AND Finalizado = 0;";

            return await _session._connection.QueryFirstOrDefaultAsync<ConversationState>(
                sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task Incluir(ConversationState state)
        {
            var sql = @"
                INSERT INTO ConversationState (
                    Id, EmpresaId, ContatoId, FlowId, EtapaAtualId,
                    Variaveis, DataInicio, DataAtualizacao, Finalizado
                ) VALUES (
                    @Id, @EmpresaId, @ContatoId, @FlowId, @EtapaAtualId,
                    @Variaveis, @DataInicio, @DataAtualizacao, @Finalizado
                );";

            await _session._connection.ExecuteAsync(sql, state, transaction: _session.Transaction);
        }

        public async Task Atualizar(ConversationState state)
        {
            var sql = @"
                UPDATE ConversationState SET
                    EtapaAtualId = @EtapaAtualId,
                    Variaveis = @Variaveis,
                    DataAtualizacao = @DataAtualizacao,
                    Finalizado = @Finalizado
                WHERE Id = @Id;";

            await _session._connection.ExecuteAsync(sql, state, transaction: _session.Transaction);
        }

        public async Task<List<ConversationState>> ObterPorFlow(Guid flowId)
        {
            var sql = @"
                SELECT * FROM ConversationState
                WHERE FlowId = @FlowId
                ORDER BY DataAtualizacao DESC;";

            var result = await _session._connection.QueryAsync<ConversationState>(
                sql, new { FlowId = flowId }, transaction: _session.Transaction);

            return result.ToList();
        }

        public async Task Excluir(Guid id)
        {
            var sql = "DELETE FROM ConversationState WHERE Id = @Id;";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }
    }
}
