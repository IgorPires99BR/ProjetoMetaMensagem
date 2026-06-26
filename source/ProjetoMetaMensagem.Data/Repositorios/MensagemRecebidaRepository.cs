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
                INSERT INTO MensagemRecebida (Id, EmpresaId, ContatoId, TelefoneRemetente, Conteudo, Tipo, DataRecebimento, Lida, FlowId)
                VALUES (@Id, @EmpresaId, @ContatoId, @TelefoneRemetente, @Conteudo, @Tipo, @DataRecebimento, @Lida, @FlowId)";

            await _session._connection.ExecuteAsync(sql, mensagem, transaction: _session.Transaction);
        }

        public async Task<List<MensagemRecebida>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = @"
                SELECT * FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId
                ORDER BY DataRecebimento DESC";

            var result = await _session._connection.QueryAsync<MensagemRecebida>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task<List<MensagemRecebida>> ListarPorContato(Guid empresaId, Guid contatoId)
        {
            var sql = @"
                SELECT * FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId
                ORDER BY DataRecebimento ASC";

            var result = await _session._connection.QueryAsync<MensagemRecebida>(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task MarcarComoLida(Guid id)
        {
            var sql = "UPDATE MensagemRecebida SET Lida = 1 WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<int> ContarNaoLidas(Guid empresaId, Guid contatoId)
        {
            var sql = @"
                SELECT COUNT(1) FROM MensagemRecebida
                WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId AND Lida = 0 AND Tipo = 'recebida'";

            return await _session._connection.ExecuteScalarAsync<int>(sql, new { EmpresaId = empresaId, ContatoId = contatoId }, transaction: _session.Transaction);
        }
    }
}
