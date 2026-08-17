using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class OrigemLeadRepository : IOrigemLeadRepository
    {
        private readonly DbSession _session;

        public OrigemLeadRepository(DbSession session) => _session = session;

        public async Task Incluir(OrigemLead origem)
        {
            // O índice único (EmpresaId, Telefone) protege contra duplicidade quando duas
            // mensagens do mesmo lead chegam quase juntas; aqui a inserção é ignorada nesse caso.
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM OrigemLead WHERE EmpresaId = @EmpresaId AND Telefone = @Telefone)
                INSERT INTO OrigemLead (Id, EmpresaId, ContatoId, Telefone, CtwaClid, SourceId, SourceType,
                                        SourceUrl, Headline, Corpo, DataPrimeiroContato, ConversaoEnviada)
                VALUES (@Id, @EmpresaId, @ContatoId, @Telefone, @CtwaClid, @SourceId, @SourceType,
                        @SourceUrl, @Headline, @Corpo, @DataPrimeiroContato, @ConversaoEnviada)";

            await _session.Connection.ExecuteAsync(sql, origem, transaction: _session.Transaction);
        }

        public async Task<OrigemLead?> ObterPorTelefone(Guid empresaId, string telefone)
        {
            var sql = @"SELECT TOP 1 * FROM OrigemLead WHERE EmpresaId = @EmpresaId AND Telefone = @Telefone";
            return await _session.Connection.QueryFirstOrDefaultAsync<OrigemLead>(
                sql, new { EmpresaId = empresaId, Telefone = telefone }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<OrigemLead>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = @"SELECT * FROM OrigemLead WHERE EmpresaId = @EmpresaId ORDER BY DataPrimeiroContato DESC";
            return await _session.Connection.QueryAsync<OrigemLead>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task MarcarConversaoEnviada(Guid id)
        {
            var sql = @"UPDATE OrigemLead SET ConversaoEnviada = 1 WHERE Id = @Id";
            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }
    }
}
