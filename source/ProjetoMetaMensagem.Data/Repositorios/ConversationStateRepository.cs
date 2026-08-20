using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ConversationStateRepository : IConversationStateRepository
    {
        private readonly DbSession _session;

        // A tabela no banco ja foi traduzida para portugues (EstadoConversa); o tipo C#
        // ConversationState continua com o nome atual ate a renomeacao de UseCases ser decidida.
        private const string Tabela = "EstadoConversa";

        public ConversationStateRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<ConversationState?> ObterPorEmpresaEContato(Guid empresaId, Guid contatoId)
        {
            var sql = $@"
                SELECT * FROM {Tabela}
                WHERE {nameof(ConversationState.EmpresaId)} = @EmpresaId
                  AND {nameof(ConversationState.ContatoId)} = @ContatoId
                  AND {nameof(ConversationState.Finalizado)} = 0;";

            return await _session.Connection.QueryFirstOrDefaultAsync<ConversationState>(
                sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task<ConversationState?> ObterAtivaParaAtualizacao(Guid empresaId, Guid contatoId)
        {
            // Aqui havia um WITH (UPDLOCK, ROWLOCK) pra serializar mensagens simultaneas do
            // mesmo cliente. Foi REVERTIDO: no teste local a primeira mensagem de uma conversa
            // nova travava a transacao (o SELECT sem resultado segurava o intervalo do indice
            // filtrado UX_EstadoConversa_Ativa e o INSERT seguinte ficava esperando), e a
            // conexao so liberava ao derrubar a API. Chat travado e pior do que resposta
            // repetida, entao a trava saiu ate existir uma forma comprovadamente segura.
            return await ObterPorEmpresaEContato(empresaId, contatoId);
        }

        // Um unico UPDATE condicional resolve a disputa: quem conseguir gravar a reserva (1 linha
        // afetada) processa, quem pegar 0 desiste. Nao ha leitura antes da escrita, entao nao ha
        // janela entre "ver que esta livre" e "reservar".
        //
        // A reserva vence sozinha (ProcessandoAte no passado): se a requisicao morrer no meio,
        // a proxima mensagem volta a ser processada em vez de a conversa ficar muda pra sempre.
        public async Task<ResultadoReserva> TentarReservarProcessamento(Guid empresaId, Guid contatoId, int segundosDeReserva)
        {
            var filtro = $@"
                {nameof(ConversationState.EmpresaId)} = @EmpresaId
                AND {nameof(ConversationState.ContatoId)} = @ContatoId
                AND {nameof(ConversationState.Finalizado)} = 0";

            // Conexao propria, fora da transacao da requisicao: a reserva precisa ser enxergada
            // pelas OUTRAS requisicoes na hora, e escrita dentro de transacao aberta so aparece
            // depois do commit.
            using var conexao = _session.AbrirConexaoIndependente();

            // Um UPDATE condicional decide a disputa sozinho: quem afetar a linha reservou.
            var reservou = await conexao.ExecuteAsync($@"
                UPDATE {Tabela}
                SET {nameof(ConversationState.ProcessandoAte)} = DATEADD(second, @Segundos, GETDATE())
                WHERE {filtro}
                  AND ({nameof(ConversationState.ProcessandoAte)} IS NULL
                       OR {nameof(ConversationState.ProcessandoAte)} < GETDATE());",
                new { EmpresaId = empresaId, ContatoId = contatoId, Segundos = segundosDeReserva });

            if (reservou == 1) return ResultadoReserva.Reservada;

            // Nada foi afetado: ou nao existe conversa (caminho de criacao, segue em frente) ou
            // existe e esta reservada por outra mensagem em processamento (desiste).
            var existe = await conexao.ExecuteScalarAsync<int>(
                $"SELECT COUNT(1) FROM {Tabela} WHERE {filtro};",
                new { EmpresaId = empresaId, ContatoId = contatoId });

            return existe > 0 ? ResultadoReserva.JaEmProcessamento : ResultadoReserva.SemConversaAinda;
        }

        public async Task LiberarProcessamento(Guid empresaId, Guid contatoId)
        {
            var sql = $@"
                UPDATE {Tabela} SET {nameof(ConversationState.ProcessandoAte)} = NULL
                WHERE {nameof(ConversationState.EmpresaId)} = @EmpresaId
                  AND {nameof(ConversationState.ContatoId)} = @ContatoId;";

            using var conexao = _session.AbrirConexaoIndependente();
            await conexao.ExecuteAsync(sql, new { EmpresaId = empresaId, ContatoId = contatoId });
        }

        public async Task Incluir(ConversationState state)
        {
            var sql = $@"
                INSERT INTO {Tabela} (
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
                UPDATE {Tabela} SET
                    {nameof(ConversationState.EtapaAtualId)} = @EtapaAtualId,
                    {nameof(ConversationState.Variaveis)} = @Variaveis,
                    {nameof(ConversationState.DataAtualizacao)} = @DataAtualizacao,
                    {nameof(ConversationState.Finalizado)} = @Finalizado,
                    {nameof(ConversationState.TentativasNaEtapa)} = @TentativasNaEtapa,
                    {nameof(ConversationState.AguardandoAtendente)} = @AguardandoAtendente
                WHERE {nameof(ConversationState.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(sql, state, transaction: _session.Transaction);
        }

        public async Task<List<ConversationState>> ObterPorFlow(Guid flowId)
        {
            var sql = $@"
                SELECT * FROM {Tabela}
                WHERE {nameof(ConversationState.FlowId)} = @FlowId
                ORDER BY {nameof(ConversationState.DataAtualizacao)} DESC;";

            var result = await _session.Connection.QueryAsync<ConversationState>(
                sql, new { FlowId = flowId }, transaction: _session.Transaction);

            return result.ToList();
        }

        public async Task Excluir(Guid id)
        {
            var sql = $"DELETE FROM {Tabela} WHERE {nameof(ConversationState.Id)} = @Id;";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<ConversationState>> ObterAtivasPorEmpresaEContatos(Guid empresaId, List<Guid> contatoIds)
        {
            var idsLista = contatoIds.Distinct().ToList();
            if (idsLista.Count == 0) return new List<ConversationState>();

            var sql = $@"
                SELECT * FROM {Tabela}
                WHERE {nameof(ConversationState.EmpresaId)} = @EmpresaId
                  AND {nameof(ConversationState.ContatoId)} IN @ContatoIds
                  AND {nameof(ConversationState.Finalizado)} = 0;";

            var result = await _session.Connection.QueryAsync<ConversationState>(
                sql, new { EmpresaId = empresaId, ContatoIds = idsLista }, transaction: _session.Transaction);

            return result.ToList();
        }

        public async Task AssumirManualmente(Guid id, Guid usuarioId)
        {
            var sql = $@"
                UPDATE {Tabela} SET
                    {nameof(ConversationState.AssumidoPorUsuarioId)} = @UsuarioId,
                    {nameof(ConversationState.DataAssumido)} = @DataAssumido
                WHERE {nameof(ConversationState.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(
                sql, new { Id = id, UsuarioId = usuarioId, DataAssumido = DateTime.Now }, transaction: _session.Transaction);
        }

        public async Task DevolverAoBot(Guid id)
        {
            var sql = $@"
                UPDATE {Tabela} SET
                    {nameof(ConversationState.AssumidoPorUsuarioId)} = NULL,
                    {nameof(ConversationState.DataAssumido)} = NULL,
                    -- Zera a contagem junto: sem isso o bot volta ja no limite e desiste de
                    -- novo na primeira resposta fora do esperado, devolvendo pro atendente.
                    {nameof(ConversationState.TentativasNaEtapa)} = 0,
                    {nameof(ConversationState.AguardandoAtendente)} = 0
                WHERE {nameof(ConversationState.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }
    }
}

