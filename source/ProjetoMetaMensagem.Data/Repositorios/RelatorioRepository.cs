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
                DataInicio = dataInicio,
                DataFim = dataFim,
                Skip = pagina * tamanhoPagina,
                Take = tamanhoPagina
            };

            var resultado = await _session.Connection.QueryAsync<RelatorioMensagemDto>(sql, parametros, transaction: _session.Transaction);
            return resultado.ToList();
        }
    }
}
