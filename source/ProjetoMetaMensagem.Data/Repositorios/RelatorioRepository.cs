using Dapper;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly DbSession _session;

        public RelatorioRepository(DbSession session)
        {
            _session = session;
        }

        // UNION ALL entre HistoricoDisparo (enviadas) e MensagemRecebida (recebidas), pra dar
        // uma visão única da conversa de cada empresa com os números de origem/destino de cada lado.
        public async Task<List<RelatorioMensagemDto>> ListarMensagens(Guid empresaId, DateTime? dataInicio, DateTime? dataFim, int pagina, int tamanhoPagina)
        {
            var sql = @"
                SELECT
                    'Enviada' AS Direcao,
                    e.Telefone AS NumeroOrigem,
                    ISNULL(c.Telefone, '') AS NumeroDestino,
                    h.Conteudo AS Conteudo,
                    h.DataEnvio AS DataHora,
                    h.StatusEntrega AS Status
                FROM HistoricoDisparo h
                INNER JOIN Empresa e ON e.Id = h.EmpresaId
                LEFT JOIN Contato c ON c.Id = h.ContatoId
                WHERE h.EmpresaId = @EmpresaId
                  AND (@DataInicio IS NULL OR h.DataEnvio >= @DataInicio)
                  AND (@DataFim IS NULL OR h.DataEnvio <= @DataFim)

                UNION ALL

                SELECT
                    'Recebida' AS Direcao,
                    ISNULL(m.TelefoneRemetente, '') AS NumeroOrigem,
                    e.Telefone AS NumeroDestino,
                    m.Conteudo AS Conteudo,
                    m.DataRecebimento AS DataHora,
                    NULL AS Status
                FROM MensagemRecebida m
                INNER JOIN Empresa e ON e.Id = m.EmpresaId
                WHERE m.EmpresaId = @EmpresaId
                  AND (@DataInicio IS NULL OR m.DataRecebimento >= @DataInicio)
                  AND (@DataFim IS NULL OR m.DataRecebimento <= @DataFim)

                ORDER BY DataHora DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            var parametros = new
            {
                EmpresaId = empresaId,
                DataInicio = dataInicio?.Date,
                // O <input type="date"> da tela manda a data com hora 00:00, e a comparacao e
                // "<= @DataFim". Sem empurrar pro fim do dia, filtrar "de hoje ate hoje" (o caso
                // mais comum) nao trazia NADA, porque toda mensagem do dia e posterior a 00:00.
                DataFim = dataFim?.Date.AddDays(1).AddTicks(-1),
                Skip = pagina * tamanhoPagina,
                Take = tamanhoPagina
            };

            var resultado = await _session.Connection.QueryAsync<RelatorioMensagemDto>(sql, parametros, transaction: _session.Transaction);
            return resultado.ToList();
        }

        // So entra no gasto o disparo que abre conversa paga na Meta (TipoDisparo Template/Flow
        // com um TemplateId real). "Livre" e resposta dentro da janela de atendimento -- nao
        // gera cobranca por conversa, entao fica de fora da soma.
        public async Task<List<GastoEmpresaMesDto>> ListarGastoPorEmpresaMes(Guid? empresaId, DateTime? dataInicio, DateTime? dataFim)
        {
            var sql = @"
                SELECT
                    h.EmpresaId,
                    e.Nome AS NomeEmpresa,
                    YEAR(h.DataEnvio) AS Ano,
                    MONTH(h.DataEnvio) AS Mes,
                    ISNULL(t.Categoria, 'SEM_CATEGORIA') AS Categoria,
                    COUNT(*) AS Quantidade,
                    COUNT(*) * ISNULL(p.PrecoUnitario, 0) AS GastoEstimado
                FROM HistoricoDisparo h
                INNER JOIN Empresa e ON e.Id = h.EmpresaId
                LEFT JOIN Template t ON t.Id = h.TemplateId
                LEFT JOIN PrecoTemplateCategoria p ON p.Categoria = t.Categoria
                WHERE h.TipoDisparo IN ('Template', 'Flow') AND h.TemplateId IS NOT NULL
                  AND (@EmpresaId IS NULL OR h.EmpresaId = @EmpresaId)
                  AND (@DataInicio IS NULL OR h.DataEnvio >= @DataInicio)
                  AND (@DataFim IS NULL OR h.DataEnvio <= @DataFim)
                GROUP BY h.EmpresaId, e.Nome, YEAR(h.DataEnvio), MONTH(h.DataEnvio), t.Categoria, p.PrecoUnitario
                ORDER BY Ano DESC, Mes DESC, e.Nome";

            var parametros = new
            {
                EmpresaId = empresaId,
                DataInicio = dataInicio?.Date,
                DataFim = dataFim?.Date.AddDays(1).AddTicks(-1)
            };

            var resultado = await _session.Connection.QueryAsync<GastoEmpresaMesDto>(sql, parametros, transaction: _session.Transaction);
            return resultado.ToList();
        }

        // Funil por empresa: quem recebeu disparo, quem visualizou (StatusEntrega = read) e quem
        // respondeu depois (existe MensagemRecebida do mesmo telefone, apos o primeiro disparo).
        public async Task<List<EngajamentoEmpresaDto>> ListarEngajamento(Guid? empresaId, DateTime? dataInicio, DateTime? dataFim)
        {
            var sql = @"
                WITH Enviados AS (
                    SELECT DISTINCT h.EmpresaId, h.ContatoId, MIN(h.DataEnvio) OVER (PARTITION BY h.EmpresaId, h.ContatoId) AS PrimeiroEnvio
                    FROM HistoricoDisparo h
                    WHERE (@EmpresaId IS NULL OR h.EmpresaId = @EmpresaId)
                      AND (@DataInicio IS NULL OR h.DataEnvio >= @DataInicio)
                      AND (@DataFim IS NULL OR h.DataEnvio <= @DataFim)
                ),
                Visualizados AS (
                    SELECT DISTINCT h.EmpresaId, h.ContatoId
                    FROM HistoricoDisparo h
                    WHERE h.StatusEntrega = 'read'
                      AND (@EmpresaId IS NULL OR h.EmpresaId = @EmpresaId)
                      AND (@DataInicio IS NULL OR h.DataEnvio >= @DataInicio)
                      AND (@DataFim IS NULL OR h.DataEnvio <= @DataFim)
                ),
                Respondidos AS (
                    SELECT DISTINCT en.EmpresaId, en.ContatoId
                    FROM Enviados en
                    INNER JOIN Contato c ON c.Id = en.ContatoId
                    INNER JOIN MensagemRecebida m
                        ON m.EmpresaId = en.EmpresaId
                       AND m.TelefoneRemetente = c.Telefone
                       AND m.DataRecebimento >= en.PrimeiroEnvio
                )
                SELECT
                    e.Id AS EmpresaId,
                    e.Nome AS NomeEmpresa,
                    (SELECT COUNT(DISTINCT en.ContatoId) FROM Enviados en WHERE en.EmpresaId = e.Id) AS Enviados,
                    (SELECT COUNT(DISTINCT v.ContatoId) FROM Visualizados v WHERE v.EmpresaId = e.Id) AS Visualizaram,
                    (SELECT COUNT(DISTINCT r.ContatoId) FROM Respondidos r WHERE r.EmpresaId = e.Id) AS Responderam
                FROM Empresa e
                WHERE (@EmpresaId IS NULL OR e.Id = @EmpresaId)
                  AND EXISTS (SELECT 1 FROM Enviados en2 WHERE en2.EmpresaId = e.Id)
                ORDER BY e.Nome";

            var parametros = new
            {
                EmpresaId = empresaId,
                DataInicio = dataInicio?.Date,
                DataFim = dataFim?.Date.AddDays(1).AddTicks(-1)
            };

            var resultado = await _session.Connection.QueryAsync<EngajamentoEmpresaDto>(sql, parametros, transaction: _session.Transaction);
            return resultado.ToList();
        }

        public async Task<List<PrecoCategoriaDto>> ListarPrecosCategoria()
        {
            var sql = "SELECT Categoria, PrecoUnitario, Moeda FROM PrecoTemplateCategoria ORDER BY Categoria";
            var resultado = await _session.Connection.QueryAsync<PrecoCategoriaDto>(sql, transaction: _session.Transaction);
            return resultado.ToList();
        }

        public async Task AtualizarPrecoCategoria(string categoria, decimal precoUnitario)
        {
            var sql = @"
                UPDATE PrecoTemplateCategoria
                SET PrecoUnitario = @PrecoUnitario, DataAtualizacao = GETDATE()
                WHERE Categoria = @Categoria";

            await _session.Connection.ExecuteAsync(sql, new { Categoria = categoria, PrecoUnitario = precoUnitario }, transaction: _session.Transaction);
        }
    }
}
