using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class CobrancaClienteRepository : ICobrancaClienteRepository
    {
        private readonly DbSession _session;

        public CobrancaClienteRepository(DbSession session) => _session = session;

        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task Incluir(CobrancaCliente cobranca)
        {
            var sql = @"
                INSERT INTO CobrancaCliente (Id, EmpresaId, ContatoId, TemplateId, HistoricoDisparoId,
                                             Valor, DataVencimento, Status, DataPagamento,
                                             UtmContentCakto, EventoIdCakto, DataCriacao,
                                             Txid, PixCopiaECola, PixExpiraEm, Multa, Juros, ValorCobrado)
                VALUES (@Id, @EmpresaId, @ContatoId, @TemplateId, @HistoricoDisparoId,
                        @Valor, @DataVencimento, @Status, @DataPagamento,
                        @UtmContentCakto, @EventoIdCakto, @DataCriacao,
                        @Txid, @PixCopiaECola, @PixExpiraEm, @Multa, @Juros, @ValorCobrado)";

            await _session.Connection.ExecuteAsync(sql, cobranca, transaction: _session.Transaction);
        }

        public async Task<CobrancaCliente?> ObterPorId(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                SELECT * FROM CobrancaCliente
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.QueryFirstOrDefaultAsync<CobrancaCliente>(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        // status = "VENCIDA" e um filtro calculado (Pendente + vencimento no passado), nao um
        // valor gravado na coluna -- ainda nao existe nenhum job que transicione o Status pra
        // isso (fica pra quando o disparo de recorrencia for construido).
        public async Task<IEnumerable<CobrancaCliente>> ObterPorEmpresa(Guid? empresaIdSolicitante, string? status,
            DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var sql = $@"
                SELECT * FROM CobrancaCliente
                WHERE 1 = 1
                {RecorteDaEmpresa}
                AND (
                    @Status IS NULL
                    OR (@Status = 'VENCIDA' AND Status = 'PENDENTE' AND DataVencimento < CAST(GETDATE() AS DATE))
                    OR (@Status <> 'VENCIDA' AND Status = @Status)
                )
                AND (@DataInicio IS NULL OR DataCriacao >= @DataInicio)
                AND (@DataFim IS NULL OR DataCriacao <= @DataFim)
                ORDER BY DataVencimento ASC";

            return await _session.Connection.QueryAsync<CobrancaCliente>(sql,
                new
                {
                    EmpresaIdSolicitante = empresaIdSolicitante,
                    Status = status,
                    DataInicio = dataInicio?.Date,
                    // <input type="date"> manda 00:00; sem empurrar pro fim do dia, "de hoje ate
                    // hoje" nao traria nada (mesmo caso do RelatorioRepository.ListarMensagens).
                    DataFim = dataFim?.Date.AddDays(1).AddTicks(-1)
                },
                transaction: _session.Transaction);
        }

        public async Task<int> MarcarPaga(Guid id, Guid? empresaIdSolicitante, DateTime dataPagamento)
        {
            var sql = $@"
                UPDATE CobrancaCliente
                SET Status = 'PAGA', DataPagamento = @DataPagamento, DataAtualizacao = @DataAtualizacao
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, DataPagamento = dataPagamento, DataAtualizacao = DateTime.Now, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<CobrancaCliente?> ObterPorTxid(string txid)
        {
            var sql = "SELECT * FROM CobrancaCliente WHERE Txid = @Txid";
            return await _session.Connection.QueryFirstOrDefaultAsync<CobrancaCliente>(sql, new { Txid = txid }, transaction: _session.Transaction);
        }

        // Pendentes cujo Pix ainda pode ser pago (ou acabou de expirar: o pagamento feito no
        // ultimo minuto pode chegar no Itau depois da expiracao).
        public async Task<IEnumerable<CobrancaCliente>> ObterPendentesComPix(DateTime expiradoDesde)
        {
            var sql = @"
                SELECT * FROM CobrancaCliente
                WHERE Status = 'PENDENTE'
                  AND Txid IS NOT NULL
                  AND PixExpiraEm >= @ExpiradoDesde";

            return await _session.Connection.QueryAsync<CobrancaCliente>(sql, new { ExpiradoDesde = expiradoDesde }, transaction: _session.Transaction);
        }

        // Condicionado a PENDENTE: webhook e consulta periodica podem chegar juntos, e uma baixa
        // manual feita antes nao deve ser sobrescrita.
        public async Task<int> BaixarPix(Guid id, DateTime dataPagamento, decimal valorPago, string? endToEndId)
        {
            var sql = @"
                UPDATE CobrancaCliente
                SET Status = 'PAGA', DataPagamento = @DataPagamento, ValorPago = @ValorPago,
                    EndToEndId = @EndToEndId, DataAtualizacao = @DataAtualizacao
                WHERE Id = @Id AND Status = 'PENDENTE'";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, DataPagamento = dataPagamento, ValorPago = valorPago, EndToEndId = endToEndId, DataAtualizacao = DateTime.Now },
                transaction: _session.Transaction);
        }
    }
}
