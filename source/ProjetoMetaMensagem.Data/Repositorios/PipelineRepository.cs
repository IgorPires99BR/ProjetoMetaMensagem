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
            await _session.Connection.ExecuteAsync(sql, pipeline, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar pipeline de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        // PipelineEtapa nao guarda EmpresaId: o vinculo passa pelo Pipeline.
        private const string RecorteDaEmpresaPelaEtapa = @"
              AND (@EmpresaIdSolicitante IS NULL
                   OR PipelineId IN (SELECT Id FROM Pipeline WHERE EmpresaId = @EmpresaIdSolicitante))";

        public async Task<int> Alterar(Pipeline pipeline, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE Pipeline SET Nome = @Nome
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { pipeline.Nome, pipeline.Id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            // Os leads saem primeiro: LeadPipeline aponta pra PipelineEtapa, entao apagar a etapa
            // com lead dentro batia na FK e o endpoint respondia 500 -- so nao aparecia antes
            // porque as tabelas do CRM nem existiam. Mesmo recorte de empresa da ExcluirEtapa.
            var sqlLeads = @"
                DELETE FROM LeadPipeline
                WHERE PipelineEtapaId IN (SELECT Id FROM PipelineEtapa WHERE PipelineId = @Id)
                  AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

            await _session.Connection.ExecuteAsync(sqlLeads,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            // O mesmo escopo vai na cascata, senao as etapas de um pipeline alheio seriam
            // apagadas antes do DELETE do proprio Pipeline ser barrado.
            var sqlEtapas = $@"
                DELETE FROM PipelineEtapa
                WHERE PipelineId = @Id
                {RecorteDaEmpresaPelaEtapa}";

            await _session.Connection.ExecuteAsync(sqlEtapas,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            var sql = $@"
                DELETE FROM Pipeline
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Pipeline?> ObterPorId(Guid id)
        {
            var sql = "SELECT * FROM Pipeline WHERE Id = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Pipeline>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<Pipeline>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = "SELECT * FROM Pipeline WHERE EmpresaId = @EmpresaId ORDER BY DataCriacao DESC";
            var result = await _session.Connection.QueryAsync<Pipeline>(sql, new { empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task<List<PipelineEtapa>> ListarEtapas(Guid pipelineId)
        {
            var sql = "SELECT * FROM PipelineEtapa WHERE PipelineId = @PipelineId ORDER BY Ordem";
            var result = await _session.Connection.QueryAsync<PipelineEtapa>(sql, new { PipelineId = pipelineId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task IncluirEtapa(PipelineEtapa etapa)
        {
            var sql = @"INSERT INTO PipelineEtapa (Id, PipelineId, Nome, Ordem, Cor, DispararAoEntrar, TemplateIdAoEntrar, DataCriacao)
                        VALUES (@Id, @PipelineId, @Nome, @Ordem, @Cor, @DispararAoEntrar, @TemplateIdAoEntrar, @DataCriacao)";
            await _session.Connection.ExecuteAsync(sql, etapa, transaction: _session.Transaction);
        }

        public async Task<int> AlterarEtapa(PipelineEtapa etapa, Guid? empresaIdSolicitante)
        {
            var sql = $@"UPDATE PipelineEtapa SET Nome = @Nome, Ordem = @Ordem, Cor = @Cor,
                        DispararAoEntrar = @DispararAoEntrar, TemplateIdAoEntrar = @TemplateIdAoEntrar
                        WHERE Id = @Id
                        {RecorteDaEmpresaPelaEtapa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    etapa.Id,
                    etapa.Nome,
                    etapa.Ordem,
                    etapa.Cor,
                    etapa.DispararAoEntrar,
                    etapa.TemplateIdAoEntrar,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> ExcluirEtapa(Guid id, Guid? empresaIdSolicitante)
        {
            // LeadPipeline tem EmpresaId proprio, entao a cascata usa o recorte direto.
            var sqlLeads = @"
                DELETE FROM LeadPipeline
                WHERE PipelineEtapaId = @Id
                  AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

            await _session.Connection.ExecuteAsync(sqlLeads,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            var sql = $@"
                DELETE FROM PipelineEtapa
                WHERE Id = @Id
                {RecorteDaEmpresaPelaEtapa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<PipelineEtapa?> ObterEtapaPorId(Guid id)
        {
            var sql = "SELECT * FROM PipelineEtapa WHERE Id = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<PipelineEtapa>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<LeadPipeline>> ListarLeads(Guid empresaId)
        {
            var sql = @"SELECT lp.* FROM LeadPipeline lp
                        INNER JOIN PipelineEtapa pe ON pe.Id = lp.PipelineEtapaId
                        INNER JOIN Pipeline p ON p.Id = pe.PipelineId
                        WHERE lp.EmpresaId = @EmpresaId
                        ORDER BY lp.DataUltimaAlteracao DESC";
            var result = await _session.Connection.QueryAsync<LeadPipeline>(sql, new { empresaId }, transaction: _session.Transaction);
            return result.ToList();
        }

        public async Task IncluirLead(LeadPipeline lead)
        {
            var sql = @"INSERT INTO LeadPipeline (Id, EmpresaId, ContatoId, PipelineEtapaId, Valor, Observacao, DataEntrada, DataUltimaAlteracao, DataCriacao)
                        VALUES (@Id, @EmpresaId, @ContatoId, @PipelineEtapaId, @Valor, @Observacao, @DataEntrada, @DataUltimaAlteracao, @DataCriacao)";
            await _session.Connection.ExecuteAsync(sql, lead, transaction: _session.Transaction);
        }

        public async Task<int> MoverLead(Guid leadId, Guid novaEtapaId, Guid? empresaIdSolicitante)
        {
            // Alem de exigir que o lead seja da empresa, a etapa de destino tambem precisa ser:
            // caso contrario daria pra empurrar um lead proprio pra dentro do funil de outra
            // empresa, que passaria a ve-lo.
            var sql = @"
                UPDATE LeadPipeline
                SET PipelineEtapaId = @NovaEtapaId, DataUltimaAlteracao = GETDATE()
                WHERE Id = @LeadId
                  AND (@EmpresaIdSolicitante IS NULL
                       OR (EmpresaId = @EmpresaIdSolicitante
                           AND @NovaEtapaId IN (
                               SELECT pe.Id FROM PipelineEtapa pe
                               INNER JOIN Pipeline p ON p.Id = pe.PipelineId
                               WHERE p.EmpresaId = @EmpresaIdSolicitante)))";

            return await _session.Connection.ExecuteAsync(sql,
                new { LeadId = leadId, NovaEtapaId = novaEtapaId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<int> RemoverLead(Guid leadId, Guid? empresaIdSolicitante)
        {
            var sql = @"
                DELETE FROM LeadPipeline
                WHERE Id = @Id
                  AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = leadId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<LeadPipeline?> ObterLead(Guid id)
        {
            var sql = "SELECT * FROM LeadPipeline WHERE Id = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<LeadPipeline>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<bool> LeadJaExiste(Guid empresaId, Guid contatoId)
        {
            var sql = "SELECT COUNT(1) FROM LeadPipeline WHERE EmpresaId = @EmpresaId AND ContatoId = @ContatoId";
            var count = await _session.Connection.ExecuteScalarAsync<int>(sql, new { empresaId, contatoId }, transaction: _session.Transaction);
            return count > 0;
        }

        public async Task<int> ContarLeadsPorEtapa(Guid etapaId, Guid? empresaIdSolicitante)
        {
            // Recorte aqui tambem: sem ele, uma etapa de outra empresa devolveria a contagem
            // real e o handler responderia "etapa com leads" -- confirmando ao atacante que
            // aquele id existe. Com o recorte a contagem da 0 e o fluxo cai na mensagem
            // generica de "nao encontrada" do proprio DELETE.
            var sql = @"
                SELECT COUNT(1) FROM LeadPipeline
                WHERE PipelineEtapaId = @EtapaId
                  AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

            return await _session.Connection.ExecuteScalarAsync<int>(sql,
                new { EtapaId = etapaId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }
    }
}

