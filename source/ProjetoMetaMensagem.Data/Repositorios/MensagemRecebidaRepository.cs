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
                INSERT INTO MensagemRecebida (Id, EmpresaId, ContatoId, TelefoneRemetente, Conteudo, Tipo, DataRecebimento, Lida, FlowId, MidiaId, TipoMidia)
                VALUES (@Id, @EmpresaId, @ContatoId, @TelefoneRemetente, @Conteudo, @Tipo, @DataRecebimento, @Lida, @FlowId, @MidiaId, @TipoMidia)";

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

        public async Task<List<MensagemRecebida>> ListarPorContato(Guid empresaId, Guid contatoId)
        {
            var sql = @"
                SELECT * FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId AND (ContatoId = @ContatoId OR ContatoId IS NULL)
                ORDER BY DataRecebimento ASC";

            var result = await _session.Connection.QueryAsync<MensagemRecebida>(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task<List<MensagemRecebida>> ListarPorContatoPaginado(Guid empresaId, Guid contatoId, int pagina, int tamanhoPagina)
        {
            var sql = @"
                SELECT * FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId AND (ContatoId = @ContatoId OR ContatoId IS NULL)
                ORDER BY DataRecebimento DESC
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
    }
}

