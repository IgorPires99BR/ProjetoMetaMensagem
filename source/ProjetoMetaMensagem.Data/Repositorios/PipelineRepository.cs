using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class PipelineRepository : IPipelineRepository
    {
        private readonly DbSession _session;
        public PipelineRepository(DbSession session) => _session = session;

        public async Task Incluir(Pipeline pipeline)
        {
            var sql = @"INSERT INTO Pipeline (Id, EmpresaId, Nome, DataCriacao)
                        VALUES (@Id, @EmpresaId, @Nome, @DataCriacao)";
            await _session._connection.ExecuteAsync(sql, pipeline, transaction: _session.Transaction);
        }

        public async Task Alterar(Pipeline pipeline)
        {
            var sql = "UPDATE Pipeline SET Nome = @Nome WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, new { pipeline.Nome, pipeline.Id }, transaction: _session.Transaction);
        }

        public async Task Excluir(Guid id)
        {
            var sqlEtapas = "DELETE FROM PipelineEtapa WHERE PipelineId = @Id";
            await _session._connection.ExecuteAsync(sqlEtapas, new { Id = id }, transaction: _session.Transaction);

            var sql = "DELETE FROM Pipeline WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Pipeline?> ObterPorId(Guid id)
        {
            var sql = "SELECT * FROM Pipeline WHERE Id = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Pipeline>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<Pipeline>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = "SELECT * FROM Pipeline WHERE EmpresaId = @EmpresaId ORDER BY DataCriacao DESC";
            var result = await _session._connection.QueryAsync<Pipeline>(sql, new { empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task<List<PipelineEtapa>> ListarEtapas(Guid pipelineId)
        {
            var sql = "SELECT * FROM PipelineEtapa WHERE PipelineId = @PipelineId ORDER BY Ordem";
            var result = await _session._connection.QueryAsync<PipelineEtapa>(sql, new { PipelineId = pipelineId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task IncluirEtapa(PipelineEtapa etapa)
        {
            var sql = @"INSERT INTO PipelineEtapa (Id, PipelineId, Nome, Ordem, Cor, DispararAoEntrar, TemplateIdAoEntrar, DataCriacao)
                        VALUES (@Id, @PipelineId, @Nome, @Ordem, @Cor, @DispararAoEntrar, @TemplateIdAoEntrar, @DataCriacao)";
            await _session._connection.ExecuteAsync(sql, etapa, transaction: _session.Transaction);
        }

        public async Task AlterarEtapa(PipelineEtapa etapa)
        {
            var sql = @"UPDATE PipelineEtapa SET Nome = @Nome, Ordem = @Ordem, Cor = @Cor,
                        DispararAoEntrar = @DispararAoEntrar, TemplateIdAoEntrar = @TemplateIdAoEntrar
                        WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, etapa, transaction: _session.Transaction);
        }

        public async Task ExcluirEtapa(Guid id)
        {
            var sqlLeads = "DELETE FROM LeadPipeline WHERE PipelineEtapaId = @Id";
            await _session._connection.ExecuteAsync(sqlLeads, new { Id = id }, transaction: _session.Transaction);

            var sql = "DELETE FROM PipelineEtapa WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<PipelineEtapa?> ObterEtapaPorId(Guid id)
        {
            var sql = "SELECT * FROM PipelineEtapa WHERE Id = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<PipelineEtapa>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<LeadPipeline>> ListarLeads(Guid empresaId)
        {
            var sql = @"SELECT lp.* FROM LeadPipeline lp
                        INNER JOIN PipelineEtapa pe ON pe.Id = lp.PipelineEtapaId
                        INNER JOIN Pipeline p ON p.Id = pe.PipelineId
                        WHERE lp.EmpresaId = @EmpresaId
                        ORDER BY lp.DataUltimaAlteracao DESC";
            var result = await _session._connection.QueryAsync<LeadPipeline>(sql, new { empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task IncluirLead(LeadPipeline lead)
        {
            var sql = @"INSERT INTO LeadPipeline (Id, EmpresaId, ContatoId, PipelineEtapaId, Valor, Observacao, DataEntrada, DataUltimaAlteracao, DataCriacao)
                        VALUES (@Id, @EmpresaId, @ContatoId, @PipelineEtapaId, @Valor, @Observacao, @DataEntrada, @DataUltimaAlteracao, @DataCriacao)";
            await _session._connection.ExecuteAsync(sql, lead, transaction: _session.Transaction);
        }

        public async Task MoverLead(Guid leadId, Guid novaEtapaId)
        {
            var sql = "UPDATE LeadPipeline SET PipelineEtapaId = @NovaEtapaId, DataUltimaAlteracao = GETDATE() WHERE Id = @LeadId";
            await _session._connection.ExecuteAsync(sql, new { LeadId = leadId, NovaEtapaId = novaEtapaId }, transaction: _session.Transaction);
        }

        public async Task RemoverLead(Guid leadId)
        {
            var sql = "DELETE FROM LeadPipeline WHERE Id = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = leadId }, transaction: _session.Transaction);
        }

        public async Task<LeadPipeline?> ObterLead(Guid id)
        {
            var sql = "SELECT * FROM LeadPipeline WHERE Id = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<LeadPipeline>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<bool> LeadJaExiste(Guid empresaId, Guid contatoId)
        {
            var sql = "SELECT COUNT(1) FROM LeadPipeline WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId";
            var count = await _session._connection.ExecuteScalarAsync<int>(sql, new { empresaId, contatoId }, transaction: _session.Transaction);
            return count > 0;
        }

        public async Task<int> ContarLeadsPorEtapa(Guid etapaId)
        {
            var sql = "SELECT COUNT(1) FROM LeadPipeline WHERE PipelineEtapaId = @EtapaId";
            return await _session._connection.ExecuteScalarAsync<int>(sql, new { EtapaId = etapaId }, transaction: _session.Transaction);
        }
    }
}
