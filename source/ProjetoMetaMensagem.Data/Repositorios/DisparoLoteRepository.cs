using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class DisparoLoteRepository : IDisparoLoteRepository
    {
        private readonly DbSession _session;

        public DisparoLoteRepository(DbSession session) => _session = session;

        private const string ColunasDoItem = "Id, LoteId, ContatoId, Ordem, Status, Tentativas, Erro, EnvioJson, CobrancaJson";

        public async Task Incluir(DisparoLote lote, IEnumerable<DisparoLoteItem> itens)
        {
            await _session.Connection.ExecuteAsync(@"
                INSERT INTO DisparoLote (Id, EmpresaId, TemplateId, ChaveIdempotencia, VariaveisJson,
                                         ParametroHeaderMediaUrl, ParametrosButtonJson, DataCriacao)
                VALUES (@Id, @EmpresaId, @TemplateId, @ChaveIdempotencia, @VariaveisJson,
                        @ParametroHeaderMediaUrl, @ParametrosButtonJson, @DataCriacao)",
                lote, transaction: _session.Transaction);

            await _session.Connection.ExecuteAsync(@"
                INSERT INTO DisparoLoteItem (Id, LoteId, ContatoId, Ordem, Status, ProximaTentativaEm)
                VALUES (@Id, @LoteId, @ContatoId, @Ordem, @Status, GETUTCDATE())",
                itens, transaction: _session.Transaction);
        }

        public Task<DisparoLote?> ObterPorChave(Guid empresaId, Guid chaveIdempotencia) =>
            _session.Connection.QueryFirstOrDefaultAsync<DisparoLote?>(
                "SELECT * FROM DisparoLote WHERE EmpresaId = @EmpresaId AND ChaveIdempotencia = @Chave",
                new { EmpresaId = empresaId, Chave = chaveIdempotencia }, transaction: _session.Transaction);

        public Task<DisparoLote?> ObterPorId(Guid id) =>
            _session.Connection.QueryFirstOrDefaultAsync<DisparoLote?>(
                "SELECT * FROM DisparoLote WHERE Id = @Id", new { Id = id }, transaction: _session.Transaction);

        public async Task<IEnumerable<Guid>> ObterContatosJaNaFila(Guid templateId, IEnumerable<Guid> contatoIds)
        {
            var encontrados = new List<Guid>();
            // Em lotes: cada id vira um parametro, e o SQL Server corta em 2100.
            foreach (var lote in contatoIds.Distinct().Chunk(1000))
            {
                encontrados.AddRange(await _session.Connection.QueryAsync<Guid>(@"
                    SELECT DISTINCT i.ContatoId
                    FROM DisparoLoteItem i
                    INNER JOIN DisparoLote l ON l.Id = i.LoteId
                    WHERE l.TemplateId = @TemplateId
                      AND i.ContatoId IN @Ids
                      AND i.Status IN ('PENDENTE', 'PREPARANDO', 'ENVIANDO')",
                    new { TemplateId = templateId, Ids = lote }, transaction: _session.Transaction));
            }
            return encontrados;
        }

        // READPAST: quem ja esta reservado por outro worker e pulado, sem fila de espera por lock.
        public Task<DisparoLoteItem?> ReservarProximo(int segundosDeReserva) =>
            _session.Connection.QueryFirstOrDefaultAsync<DisparoLoteItem?>($@"
                ;WITH proximo AS (
                    SELECT TOP (1) *
                    FROM DisparoLoteItem WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE Status = 'PENDENTE' AND ProximaTentativaEm <= GETUTCDATE()
                    ORDER BY ProximaTentativaEm, Ordem
                )
                UPDATE proximo
                SET Status = 'PREPARANDO',
                    Tentativas = Tentativas + 1,
                    ReservadoAte = DATEADD(SECOND, @Segundos, GETUTCDATE()),
                    DataAtualizacao = GETUTCDATE()
                OUTPUT {string.Join(", ", ColunasDoItem.Split(", ").Select(c => "inserted." + c))};",
                new { Segundos = segundosDeReserva }, transaction: _session.Transaction);

        public Task<DisparoLoteItem?> ReservarEnvioInterrompido(int segundosDeReserva) =>
            _session.Connection.QueryFirstOrDefaultAsync<DisparoLoteItem?>($@"
                ;WITH interrompido AS (
                    SELECT TOP (1) *
                    FROM DisparoLoteItem WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE Status = 'ENVIANDO' AND ReservadoAte < GETUTCDATE()
                    ORDER BY ReservadoAte
                )
                UPDATE interrompido
                SET ReservadoAte = DATEADD(SECOND, @Segundos, GETUTCDATE()),
                    DataAtualizacao = GETUTCDATE()
                OUTPUT {string.Join(", ", ColunasDoItem.Split(", ").Select(c => "inserted." + c))};",
                new { Segundos = segundosDeReserva }, transaction: _session.Transaction);

        // O limite de tentativas tira da fila um item que derruba o processo toda vez.
        public Task<int> LiberarPreparacoesInterrompidas(int maximoDeTentativas) =>
            _session.Connection.ExecuteAsync(@"
                UPDATE DisparoLoteItem
                SET Status = CASE WHEN Tentativas >= @Maximo THEN 'FALHOU' ELSE 'PENDENTE' END,
                    Erro = CASE WHEN Tentativas >= @Maximo THEN 'Não foi possível processar este contato depois de várias tentativas.' ELSE Erro END,
                    ReservadoAte = NULL,
                    ProximaTentativaEm = GETUTCDATE(),
                    DataAtualizacao = GETUTCDATE()
                WHERE Status = 'PREPARANDO' AND ReservadoAte < GETUTCDATE()",
                new { Maximo = maximoDeTentativas }, transaction: _session.Transaction);

        public Task SalvarPreparacao(Guid itemId, string envioJson, string? cobrancaJson) =>
            _session.Connection.ExecuteAsync(@"
                UPDATE DisparoLoteItem
                SET EnvioJson = @EnvioJson, CobrancaJson = @CobrancaJson, DataAtualizacao = GETUTCDATE()
                WHERE Id = @Id",
                new { Id = itemId, EnvioJson = envioJson, CobrancaJson = cobrancaJson }, transaction: _session.Transaction);

        public Task<int> MarcarEnviando(Guid itemId, int segundosDeReserva) =>
            _session.Connection.ExecuteAsync(@"
                UPDATE DisparoLoteItem
                SET Status = 'ENVIANDO',
                    ReservadoAte = DATEADD(SECOND, @Segundos, GETUTCDATE()),
                    DataAtualizacao = GETUTCDATE()
                WHERE Id = @Id AND Status = 'PREPARANDO'",
                new { Id = itemId, Segundos = segundosDeReserva }, transaction: _session.Transaction);

        public Task AgendarNovaTentativa(Guid itemId, int segundos, string? erro) =>
            _session.Connection.ExecuteAsync(@"
                UPDATE DisparoLoteItem
                SET Status = 'PENDENTE',
                    ProximaTentativaEm = DATEADD(SECOND, @Segundos, GETUTCDATE()),
                    ReservadoAte = NULL,
                    Erro = @Erro,
                    DataAtualizacao = GETUTCDATE()
                WHERE Id = @Id",
                new { Id = itemId, Segundos = segundos, Erro = Limitar(erro) }, transaction: _session.Transaction);

        // Os JSON saem junto: tem Pix copia e cola, e ninguem mais precisa deles.
        public Task Finalizar(Guid itemId, string status, string? erro) =>
            _session.Connection.ExecuteAsync(@"
                UPDATE DisparoLoteItem
                SET Status = @Status,
                    Erro = @Erro,
                    ReservadoAte = NULL,
                    EnvioJson = NULL,
                    CobrancaJson = NULL,
                    DataAtualizacao = GETUTCDATE()
                WHERE Id = @Id",
                new { Id = itemId, Status = status, Erro = Limitar(erro) }, transaction: _session.Transaction);

        private const string ConsultaDeResumo = @"
                SELECT {0} l.Id, l.DataCriacao, COALESCE(t.NomeExibicao, t.NomeTemplate) AS NomeTemplate,
                       COUNT(*) AS Total,
                       SUM(CASE WHEN i.Status IN ('PENDENTE', 'PREPARANDO', 'ENVIANDO') THEN 1 ELSE 0 END) AS Aguardando,
                       SUM(CASE WHEN i.Status = 'ENVIADO' THEN 1 ELSE 0 END) AS Enviados,
                       SUM(CASE WHEN i.Status = 'FALHOU' THEN 1 ELSE 0 END) AS Falhas,
                       SUM(CASE WHEN i.Status = 'INCERTO' THEN 1 ELSE 0 END) AS Incertos
                FROM DisparoLote l
                INNER JOIN Template t ON t.Id = l.TemplateId
                INNER JOIN DisparoLoteItem i ON i.LoteId = l.Id
                WHERE {1}
                GROUP BY l.Id, l.DataCriacao, t.NomeExibicao, t.NomeTemplate
                ORDER BY l.DataCriacao DESC";

        public Task<IEnumerable<DisparoLoteResumo>> ListarRecentes(Guid empresaId, int quantidade) =>
            _session.Connection.QueryAsync<DisparoLoteResumo>(
                string.Format(ConsultaDeResumo, "TOP (@Quantidade)", "l.EmpresaId = @EmpresaId"),
                new { EmpresaId = empresaId, Quantidade = quantidade }, transaction: _session.Transaction);

        public Task<DisparoLoteResumo?> ObterResumo(Guid loteId, Guid? empresaIdSolicitante) =>
            _session.Connection.QueryFirstOrDefaultAsync<DisparoLoteResumo?>(
                string.Format(ConsultaDeResumo, "", "l.Id = @LoteId AND (@EmpresaIdSolicitante IS NULL OR l.EmpresaId = @EmpresaIdSolicitante)"),
                new { LoteId = loteId, EmpresaIdSolicitante = empresaIdSolicitante }, transaction: _session.Transaction);

        // Falhas, incertos e enviados com ressalva (ex: o segundo telefone do contato nao recebeu).
        public Task<IEnumerable<DisparoLotePendencia>> ObterPendencias(Guid loteId) =>
            _session.Connection.QueryAsync<DisparoLotePendencia>(@"
                SELECT i.ContatoId,
                       COALESCE(NULLIF(LTRIM(RTRIM(c.NomeCliente)), ''), NULLIF(LTRIM(RTRIM(c.NomeContato)), ''), 'Contato sem nome') AS Nome,
                       c.Telefone, i.Status, i.Erro
                FROM DisparoLoteItem i
                INNER JOIN Contato c ON c.Id = i.ContatoId
                WHERE i.LoteId = @LoteId
                  AND (i.Status IN ('FALHOU', 'INCERTO') OR (i.Status = 'ENVIADO' AND i.Erro IS NOT NULL))
                ORDER BY i.Ordem",
                new { LoteId = loteId }, transaction: _session.Transaction);

        private static string? Limitar(string? erro) =>
            erro != null && erro.Length > 1000 ? erro[..1000] : erro;
    }
}
