using Dapper;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DbSession _session;

        public DashboardRepository(DbSession session)
            => _session = session;

        public async Task<ObterMetricasDashboardResult> ObterMetricas(Guid empresaId)
        {
            var result = new ObterMetricasDashboardResult();

            var sqlHoje = @"SELECT COUNT(1) FROM HistoricoDisparo
                            WHERE EmpresaId = @EmpresaId AND CONVERT(DATE, DataEnvio) = CONVERT(DATE, GETDATE())";
            result.MensagensHoje = await _session._connection.ExecuteScalarAsync<int>(sqlHoje, new { empresaId }, transaction: _session.Transaction);

            var sqlSemana = @"SELECT COUNT(1) FROM HistoricoDisparo
                              WHERE EmpresaId = @EmpresaId AND DataEnvio >= DATEADD(DAY, -7, GETDATE())";
            result.MensagensSemana = await _session._connection.ExecuteScalarAsync<int>(sqlSemana, new { empresaId }, transaction: _session.Transaction);

            var sqlMes = @"SELECT COUNT(1) FROM HistoricoDisparo
                           WHERE EmpresaId = @EmpresaId AND DataEnvio >= DATEADD(DAY, -30, GETDATE())";
            result.MensagensMes = await _session._connection.ExecuteScalarAsync<int>(sqlMes, new { empresaId }, transaction: _session.Transaction);

            var sqlLeads = @"SELECT COUNT(1) FROM Contato
                             WHERE EmpresaId = @EmpresaId AND CONVERT(DATE, DataCadastro) = CONVERT(DATE, GETDATE())";
            result.LeadsCapturados = await _session._connection.ExecuteScalarAsync<int>(sqlLeads, new { empresaId }, transaction: _session.Transaction);

            var sqlTaxa = @"SELECT
                CASE WHEN COUNT(1) = 0 THEN 100.0
                ELSE CAST(SUM(CASE WHEN WamidMeta IS NOT NULL AND WamidMeta != '' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(1) * 100.0
                END
                FROM HistoricoDisparo WHERE EmpresaId = @EmpresaId";
            result.TaxaEntrega = Math.Round(await _session._connection.ExecuteScalarAsync<double>(sqlTaxa, new { empresaId }, transaction: _session.Transaction), 1);

            var sqlChats = @"SELECT COUNT(1) FROM Conversations WHERE EmpresaId = @EmpresaId";
            result.ChatsAtivos = await _session._connection.ExecuteScalarAsync<int>(sqlChats, new { empresaId }, transaction: _session.Transaction);

            var sqlNumeros = @"SELECT
                SUM(CASE WHEN Status = 'Ativo' THEN 1 ELSE 0 END),
                SUM(CASE WHEN Status = 'Pendente' THEN 1 ELSE 0 END),
                SUM(CASE WHEN Status = 'Bloqueado' THEN 1 ELSE 0 END)
                FROM Numero WHERE EmpresaId = @EmpresaId";
            using (var multi = await _session._connection.QueryMultipleAsync(sqlNumeros, new { empresaId }, transaction: _session.Transaction))
            {
                var row = await multi.ReadSingleAsync<dynamic>();
                result.NumerosAtivos = (int)(row?.Column0 ?? 0);
                result.NumerosPendentes = (int)(row?.Column1 ?? 0);
                result.NumerosBloqueados = (int)(row?.Column2 ?? 0);
            }

            var sqlFlows = @"SELECT COUNT(1) FROM Flow WHERE EmpresaId = @EmpresaId AND Ativo = 1";
            result.FlowsAtivos = await _session._connection.ExecuteScalarAsync<int>(sqlFlows, new { empresaId }, transaction: _session.Transaction);

            var sqlDisparosRecentes = @"
                SELECT TOP 3 c.Nome, h.Total, h.Enviadas, h.Status FROM (
                    SELECT
                        CASE WHEN TemplateId IS NOT NULL THEN (SELECT TOP 1 Nome FROM Template WHERE Id = TemplateId) ELSE 'Disparo Livre' END AS Nome,
                        COUNT(1) AS Total,
                        SUM(CASE WHEN WamidMeta IS NOT NULL AND WamidMeta != '' THEN 1 ELSE 0 END) AS Enviadas,
                        'Concluído' AS Status
                    FROM HistoricoDisparo
                    WHERE EmpresaId = @EmpresaId
                    GROUP BY TemplateId
                ) c ORDER BY c.Total DESC";
            var disparos = await _session._connection.QueryAsync<DisparoRecente>(sqlDisparosRecentes, new { empresaId }, transaction: _session.Transaction);
            result.DisparosRecentes = disparos.ToList();

            var sqlFlowsAtivos = @"
                SELECT f.Nome, f.Ativo, ISNULL(e.DisparosHoje, 0) AS DisparosHoje
                FROM Flow f
                LEFT JOIN (
                    SELECT FlowId, COUNT(1) AS DisparosHoje
                    FROM ConversationState
                    WHERE EmpresaId = @EmpresaId AND CONVERT(DATE, DataCriacao) = CONVERT(DATE, GETDATE())
                    GROUP BY FlowId
                ) e ON e.FlowId = f.Id
                WHERE f.EmpresaId = @EmpresaId";
            var flows = await _session._connection.QueryAsync<FlowAtivo>(sqlFlowsAtivos, new { empresaId }, transaction: _session.Transaction);
            result.FlowsComExecucoes = flows.ToList();

            var sqlEvolucao = @"
                SELECT CONVERT(VARCHAR(10), DataEnvio, 103) AS Data, COUNT(1) AS Total
                FROM HistoricoDisparo
                WHERE EmpresaId = @EmpresaId AND DataEnvio >= DATEADD(DAY, -7, GETDATE())
                GROUP BY CONVERT(VARCHAR(10), DataEnvio, 103)
                ORDER BY CONVERT(VARCHAR(10), DataEnvio, 103)";
            var evolucao = await _session._connection.QueryAsync<EvolucaoDisparo>(sqlEvolucao, new { empresaId }, transaction: _session.Transaction);
            result.EvolucaoDisparos = evolucao.ToList();

            return result;
        }
    }
}
