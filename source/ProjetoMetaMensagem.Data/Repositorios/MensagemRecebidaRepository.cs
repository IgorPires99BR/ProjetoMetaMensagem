using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class MensagemRecebidaRepository : IMensagemRecebidaRepository
    {
        private readonly DbSession _session;

        public MensagemRecebidaRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(MensagemRecebida mensagem)
        {
            var sql = @"
                INSERT INTO MensagemRecebida (Id, EmpresaId, ContatoId, TelefoneRemetente, Conteudo, Tipo, DataRecebimento, Lida, FlowId, MidiaId, TipoMidia, WamidRecebido)
                VALUES (@Id, @EmpresaId, @ContatoId, @TelefoneRemetente, @Conteudo, @Tipo, @DataRecebimento, @Lida, @FlowId, @MidiaId, @TipoMidia, @WamidRecebido)";

            await _session.Connection.ExecuteAsync(sql, mensagem, transaction: _session.Transaction);
        }

        public async Task<List<MensagemRecebida>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = @"
                SELECT * FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId
                ORDER BY DataRecebimento DESC";

            var result = await _session.Connection.QueryAsync<MensagemRecebida>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        // Mensagem sem ContatoId (numero que escreveu antes de virar cadastro) so pode entrar
        // nesta conversa se o telefone do remetente for o mesmo do contato. O criterio antigo
        // era "ContatoId = @ContatoId OR ContatoId IS NULL", o que jogava toda mensagem de
        // numero desconhecido dentro de TODAS as conversas da empresa.
        private const string FiltroDaConversa = @"
                WHERE m.EmpresaId = @EmpresaId
                  AND ( m.ContatoId = @ContatoId
                        OR ( m.ContatoId IS NULL
                             AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(m.TelefoneRemetente, '+', ''), ' ', ''), '-', ''), '(', ''), ')', '')
                                 = (SELECT REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(c.Telefone, '+', ''), ' ', ''), '-', ''), '(', ''), ')', '')
                                    FROM Contato c WHERE c.Id = @ContatoId) ) )";

        public async Task<List<MensagemRecebida>> ListarPorContato(Guid empresaId, Guid contatoId)
        {
            var sql = $@"
                SELECT m.* FROM MensagemRecebida m
                {FiltroDaConversa}
                ORDER BY m.DataRecebimento ASC";

            var result = await _session.Connection.QueryAsync<MensagemRecebida>(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task<List<MensagemRecebida>> ListarPorContatoPaginado(Guid empresaId, Guid contatoId, int pagina, int tamanhoPagina)
        {
            var sql = $@"
                SELECT m.* FROM MensagemRecebida m
                {FiltroDaConversa}
                ORDER BY m.DataRecebimento DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            var result = await _session.Connection.QueryAsync<MensagemRecebida>(sql,
                new { EmpresaId = empresaId, ContatoId = contatoId, Skip = pagina * tamanhoPagina, Take = tamanhoPagina },
                transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task MarcarComoLida(Guid id)
        {
            var sql = "UPDATE MensagemRecebida SET Lida = 1 WHERE Id = @Id";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task MarcarTodasComoLidas(Guid empresaId, Guid contatoId)
        {
            var sql = @"
                UPDATE MensagemRecebida SET Lida = 1
                WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId AND Lida = 0 AND Tipo = 'recebida'";

            await _session.Connection.ExecuteAsync(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task<int> ContarNaoLidas(Guid empresaId, Guid contatoId)
        {
            var sql = @"
                SELECT COUNT(1) FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId AND Lida = 0 AND Tipo = 'recebida'";

            return await _session.Connection.ExecuteScalarAsync<int>(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }

        public async Task<bool> ExisteMidiaId(Guid empresaId, string midiaId)
        {
            var sql = @"SELECT COUNT(1) FROM MensagemRecebida WHERE EmpresaId = @EmpresaId AND MidiaId = @MidiaId";
            var count = await _session.Connection.ExecuteScalarAsync<int>(
                sql, new { EmpresaId = empresaId, MidiaId = midiaId }, transaction: _session.Transaction);
            return count > 0;
        }

        public async Task<bool> ExistePorWamid(Guid empresaId, string wamid)
        {
            var sql = @"SELECT COUNT(1) FROM MensagemRecebida WHERE EmpresaId = @EmpresaId AND WamidRecebido = @Wamid";
            var count = await _session.Connection.ExecuteScalarAsync<int>(
                sql, new { EmpresaId = empresaId, Wamid = wamid }, transaction: _session.Transaction);
            return count > 0;
        }

        public async Task<List<ItemConversaUnificado>> ListarConversaUnificadaPaginada(Guid empresaId, Guid contatoId, int pagina, int tamanhoPagina)
        {
            var sql = $@"
                ;WITH Unificado AS (
                    SELECT
                        m.Id,
                        'user' AS Origem,
                        m.Conteudo AS Texto,
                        m.DataRecebimento AS Data,
                        CAST(NULL AS NVARCHAR(100)) AS Wamid,
                        CAST(NULL AS NVARCHAR(50)) AS Status,
                        CAST(NULL AS NVARCHAR(500)) AS Erro,
                        m.MidiaId,
                        m.TipoMidia
                    FROM MensagemRecebida m
                    {FiltroDaConversa}
                    UNION ALL
                    SELECT
                        h.Id,
                        'bot' AS Origem,
                        h.Conteudo AS Texto,
                        h.DataEnvio AS Data,
                        h.WamidMeta AS Wamid,
                        h.StatusEntrega AS Status,
                        h.MotivoFalha AS Erro,
                        h.MidiaId,
                        h.TipoMidia
                    FROM HistoricoDisparo h
                    WHERE h.EmpresaId = @EmpresaId AND h.ContatoId = @ContatoId
                )
                SELECT * FROM Unificado
                ORDER BY Data DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            var result = await _session.Connection.QueryAsync<ItemConversaUnificado>(sql,
                new { EmpresaId = empresaId, ContatoId = contatoId, Skip = pagina * tamanhoPagina, Take = tamanhoPagina },
                transaction: _session.Transaction);
            return result.ToList();
        }
    }
}

