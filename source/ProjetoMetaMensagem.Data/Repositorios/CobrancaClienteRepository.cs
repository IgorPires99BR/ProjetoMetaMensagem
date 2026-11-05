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
                                             Txid, PixIdExterno, PixCopiaECola, PixExpiraEm, Multa, Juros, ValorCobrado,
                                             ValorComissao, ValorRepasse, RecebidaNaPlataforma,
                                             Provedor, LinkPagamentoExterno)
                VALUES (@Id, @EmpresaId, @ContatoId, @TemplateId, @HistoricoDisparoId,
                        @Valor, @DataVencimento, @Status, @DataPagamento,
                        @UtmContentCakto, @EventoIdCakto, @DataCriacao,
                        @Txid, @PixIdExterno, @PixCopiaECola, @PixExpiraEm, @Multa, @Juros, @ValorCobrado,
                        @ValorComissao, @ValorRepasse, @RecebidaNaPlataforma,
                        @Provedor, @LinkPagamentoExterno)";

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

        // status = "VENCIDA" pega as gravadas assim (recorrencia enviada) e as pendentes com
        // vencimento no passado, que continuam sem job nenhum transicionando o Status.
        public async Task<IEnumerable<CobrancaCliente>> ObterPorEmpresa(Guid? empresaIdSolicitante, string? status,
            DateTime? dataInicio = null, DateTime? dataFim = null, bool apenasInativas = false)
        {
            var sql = $@"
                SELECT * FROM CobrancaCliente
                WHERE 1 = 1
                {RecorteDaEmpresa}
                AND Ativo = @Ativo
                AND (
                    @Status IS NULL
                    OR (@Status = 'VENCIDA' AND (Status = 'VENCIDA' OR (Status = 'PENDENTE' AND DataVencimento < CAST(GETDATE() AS DATE))))
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
                    Ativo = !apenasInativas,
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

        // Em aberto cujo Pix ainda pode ser pago (ou acabou de expirar: o pagamento feito no
        // ultimo minuto pode ser liquidado depois da expiracao). Inativa tambem entra: o dinheiro
        // que cair na janela precisa dar baixa.
        public async Task<IEnumerable<CobrancaCliente>> ObterPendentesComPix(DateTime expiradoDesde)
        {
            var sql = @"
                SELECT * FROM CobrancaCliente
                WHERE Status IN ('PENDENTE', 'VENCIDA')
                  AND Txid IS NOT NULL
                  AND PixExpiraEm >= @ExpiradoDesde";

            return await _session.Connection.QueryAsync<CobrancaCliente>(sql, new { ExpiradoDesde = expiradoDesde }, transaction: _session.Transaction);
        }

        // Condicionado a estar em aberto: webhook e consulta periodica podem chegar juntos, e uma
        // baixa manual feita antes nao deve ser sobrescrita.
        public async Task<int> BaixarPix(Guid id, DateTime dataPagamento, decimal valorPago, string? endToEndId)
        {
            var sql = @"
                UPDATE CobrancaCliente
                SET Status = 'PAGA', DataPagamento = @DataPagamento, ValorPago = @ValorPago,
                    EndToEndId = @EndToEndId, DataAtualizacao = @DataAtualizacao
                WHERE Id = @Id AND Status IN ('PENDENTE', 'VENCIDA')";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, DataPagamento = dataPagamento, ValorPago = valorPago, EndToEndId = endToEndId, DataAtualizacao = DateTime.Now },
                transaction: _session.Transaction);
        }

        public async Task<int> AtualizarPix(CobrancaCliente cobranca)
        {
            var sql = @"
                UPDATE CobrancaCliente
                SET Txid = @Txid, PixIdExterno = @PixIdExterno, PixCopiaECola = @PixCopiaECola, PixExpiraEm = @PixExpiraEm,
                    Multa = @Multa, Juros = @Juros, ValorCobrado = @ValorCobrado,
                    ValorComissao = @ValorComissao, ValorRepasse = @ValorRepasse, RecebidaNaPlataforma = @RecebidaNaPlataforma,
                    Provedor = @Provedor, LinkPagamentoExterno = @LinkPagamentoExterno, DataAtualizacao = @DataAtualizacao
                WHERE Id = @Id AND Status IN ('PENDENTE', 'VENCIDA')";

            cobranca.DataAtualizacao = DateTime.Now;
            return await _session.Connection.ExecuteAsync(sql, cobranca, transaction: _session.Transaction);
        }

        public async Task<int> MarcarVencida(Guid id)
        {
            var sql = @"
                UPDATE CobrancaCliente
                SET Status = 'VENCIDA', DataAtualizacao = @DataAtualizacao
                WHERE Id = @Id AND Status = 'PENDENTE'";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, DataAtualizacao = DateTime.Now },
                transaction: _session.Transaction);
        }

        // Validade do Pix em aberto vai para agora: o Pix ja foi cancelado no provedor, entao a
        // pagina de pagamento nao pode mais oferece-lo e, se a cobranca for reativada, a
        // recorrencia precisa gerar outro em vez de reenviar este.
        public async Task<int> Inativar(Guid id, Guid? empresaIdSolicitante, DateTime agora)
        {
            var sql = $@"
                UPDATE CobrancaCliente
                SET Ativo = 0, DataInativacao = @Agora, DataAtualizacao = @Agora,
                    PixExpiraEm = CASE WHEN Status IN ('PENDENTE', 'VENCIDA') AND PixExpiraEm IS NOT NULL THEN @Agora ELSE PixExpiraEm END
                WHERE Id = @Id AND Ativo = 1
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, Agora = agora, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<int> Reativar(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE CobrancaCliente
                SET Ativo = 1, DataInativacao = NULL, DataAtualizacao = @Agora
                WHERE Id = @Id AND Ativo = 0
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, Agora = DateTime.Now, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }
    }
}
