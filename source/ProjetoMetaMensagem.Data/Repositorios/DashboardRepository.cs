using Dapper;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;
using System.Linq;

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
            var conn = _session.Connection;
            var hoje = DateTime.Now.Date;
            var inicioSemana = hoje.AddDays(-7);
            var inicioMes = hoje.AddDays(-30);

            // Contadores de disparos por periodo
            var contadores = await conn.QueryFirstOrDefaultAsync<(int Hoje, int Semana, int Mes, int ComWamid, int Total)>(
                @"SELECT
                    SUM(CASE WHEN DataEnvio >= @Hoje THEN 1 ELSE 0 END) AS Hoje,
                    SUM(CASE WHEN DataEnvio >= @InicioSemana THEN 1 ELSE 0 END) AS Semana,
                    SUM(CASE WHEN DataEnvio >= @InicioMes THEN 1 ELSE 0 END) AS Mes,
                    SUM(CASE WHEN WamidMeta IS NOT NULL AND WamidMeta <> '' THEN 1 ELSE 0 END) AS ComWamid,
                    COUNT(1) AS Total
                  FROM HistoricoDisparo
                  WHERE EmpresaId = @EmpresaId AND DataEnvio >= @InicioMes",
                new { EmpresaId = empresaId, Hoje = hoje, InicioSemana = inicioSemana, InicioMes = inicioMes },
                transaction: _session.Transaction);

            result.MensagensHoje = contadores.Hoje;
            result.MensagensSemana = contadores.Semana;
            result.MensagensMes = contadores.Mes;
            result.TaxaEntrega = contadores.Total > 0
                ? Math.Round(contadores.ComWamid * 100.0 / contadores.Total, 1)
                : 0;

            // Leads: contatos distintos que enviaram ou receberam mensagem nos ultimos 30 dias
            result.LeadsCapturados = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(DISTINCT ContatoId) FROM (
                    SELECT ContatoId FROM MensagemRecebida WHERE EmpresaId = @EmpresaId AND ContatoId IS NOT NULL AND DataRecebimento >= @InicioMes
                    UNION
                    SELECT ContatoId FROM HistoricoDisparo WHERE EmpresaId = @EmpresaId AND DataEnvio >= @InicioMes
                  ) AS Leads",
                new { EmpresaId = empresaId, InicioMes = inicioMes },
                transaction: _session.Transaction);

            // Chats ativos: contatos com alguma interacao nos ultimos 7 dias
            result.ChatsAtivos = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(DISTINCT ContatoId) FROM (
                    SELECT ContatoId FROM MensagemRecebida WHERE EmpresaId = @EmpresaId AND ContatoId IS NOT NULL AND DataRecebimento >= @InicioSemana
                    UNION
                    SELECT ContatoId FROM HistoricoDisparo WHERE EmpresaId = @EmpresaId AND DataEnvio >= @InicioSemana
                  ) AS Chats",
                new { EmpresaId = empresaId, InicioSemana = inicioSemana },
                transaction: _session.Transaction);

            // Numeros por status (Numero nao tem EmpresaId direto, so via Usuario)
            var numeros = await conn.QueryAsync<string>(
                @"SELECT n.StatusMeta
                  FROM Numero n
                  INNER JOIN Usuario u ON u.Id = n.UsuarioId
                  WHERE u.EmpresaId = @EmpresaId",
                new { EmpresaId = empresaId },
                transaction: _session.Transaction);

            foreach (var status in numeros)
            {
                var s = status?.ToUpper() ?? "";
                if (s is "CONNECTED" or "APPROVED" or "LIVE") result.NumerosAtivos++;
                else if (s == "PENDING") result.NumerosPendentes++;
                else if (s is "DISCONNECTED" or "FLAGGED" or "BLOCKED") result.NumerosBloqueados++;
            }

            // Flows ativos
            result.FlowsAtivos = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Fluxo WHERE EmpresaId = @EmpresaId AND Ativo = 1",
                new { EmpresaId = empresaId },
                transaction: _session.Transaction);

            result.FlowsComExecucoes = (await conn.QueryAsync<FlowAtivo>(
                @"SELECT Nome, Ativo, 0 AS DisparosHoje FROM Fluxo WHERE EmpresaId = @EmpresaId ORDER BY DataCriacao DESC",
                new { EmpresaId = empresaId },
                transaction: _session.Transaction)).ToList();

            // Disparos recentes (individuais, mais recentes primeiro)
            result.DisparosRecentes = (await conn.QueryAsync<DisparoRecente>(
                @"SELECT TOP 5
                    ISNULL(t.NomeTemplate, h.TipoDisparo) AS Nome,
                    1 AS Enviadas,
                    1 AS Total,
                    CASE WHEN h.WamidMeta IS NOT NULL AND h.WamidMeta <> '' THEN 'Concluído' ELSE 'Pendente' END AS Status
                  FROM HistoricoDisparo h
                  LEFT JOIN Template t ON t.Id = h.TemplateId
                  WHERE h.EmpresaId = @EmpresaId
                  ORDER BY h.DataEnvio DESC",
                new { EmpresaId = empresaId },
                transaction: _session.Transaction)).ToList();

            // Evolucao de disparos nos ultimos 7 dias (para grafico)
            var evolucaoRaw = (await conn.QueryAsync<(DateTime Dia, int Total)>(
                @"SELECT CAST(DataEnvio AS DATE) AS Dia, COUNT(1) AS Total
                  FROM HistoricoDisparo
                  WHERE EmpresaId = @EmpresaId AND DataEnvio >= @InicioSemana
                  GROUP BY CAST(DataEnvio AS DATE)
                  ORDER BY Dia ASC",
                new { EmpresaId = empresaId, InicioSemana = inicioSemana },
                transaction: _session.Transaction)).ToDictionary(x => x.Dia.Date, x => x.Total);

            for (var dia = inicioSemana.Date; dia <= hoje; dia = dia.AddDays(1))
            {
                result.EvolucaoDisparos.Add(new EvolucaoDisparo
                {
                    Data = dia.ToString("dd/MM"),
                    Total = evolucaoRaw.TryGetValue(dia, out var total) ? total : 0
                });
            }

            return result;
        }
    }
}
