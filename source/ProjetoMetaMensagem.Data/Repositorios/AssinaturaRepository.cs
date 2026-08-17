using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class AssinaturaRepository : IAssinaturaRepository
    {
        private readonly DbSession _session;

        public AssinaturaRepository(DbSession session) => _session = session;

        public async Task Incluir(Assinatura assinatura)
        {
            var sql = @"
                INSERT INTO Assinatura (Id, EmpresaId, AssinaturaIdCakto, ClienteIdCakto, OfertaIdCakto,
                                        EmailComprador, Plano, Status, ValorCentavos, DataInicio,
                                        DataProximaCobranca, DataCancelamento, EventoIdCakto, UltimoEvento,
                                        DataUltimoEvento, DataCriacao)
                VALUES (@Id, @EmpresaId, @AssinaturaIdCakto, @ClienteIdCakto, @OfertaIdCakto,
                        @EmailComprador, @Plano, @Status, @ValorCentavos, @DataInicio,
                        @DataProximaCobranca, @DataCancelamento, @EventoIdCakto, @UltimoEvento,
                        @DataUltimoEvento, @DataCriacao)";

            await _session.Connection.ExecuteAsync(sql, assinatura, transaction: _session.Transaction);
        }

        public async Task<int> Alterar(Assinatura assinatura)
        {
            assinatura.DataAtualizacao = DateTime.Now;

            var sql = @"
                UPDATE Assinatura SET
                    AssinaturaIdCakto   = @AssinaturaIdCakto,
                    ClienteIdCakto      = @ClienteIdCakto,
                    OfertaIdCakto       = @OfertaIdCakto,
                    EmailComprador      = @EmailComprador,
                    Plano               = @Plano,
                    Status              = @Status,
                    ValorCentavos       = @ValorCentavos,
                    DataProximaCobranca = @DataProximaCobranca,
                    DataCancelamento    = @DataCancelamento,
                    EventoIdCakto       = @EventoIdCakto,
                    UltimoEvento        = @UltimoEvento,
                    DataUltimoEvento    = @DataUltimoEvento,
                    DataAtualizacao     = @DataAtualizacao
                WHERE Id = @Id";

            return await _session.Connection.ExecuteAsync(sql, assinatura, transaction: _session.Transaction);
        }

        public async Task<Assinatura?> ObterPorEmpresa(Guid empresaId)
        {
            // Uma empresa pode ter historico de assinaturas (cancelou e voltou): vale a mais recente.
            var sql = @"SELECT TOP 1 * FROM Assinatura WHERE EmpresaId = @EmpresaId ORDER BY DataCriacao DESC";
            return await _session.Connection.QueryFirstOrDefaultAsync<Assinatura>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<Assinatura?> ObterPorAssinaturaCakto(string assinaturaIdCakto)
        {
            var sql = @"SELECT TOP 1 * FROM Assinatura WHERE AssinaturaIdCakto = @AssinaturaIdCakto";
            return await _session.Connection.QueryFirstOrDefaultAsync<Assinatura>(
                sql, new { AssinaturaIdCakto = assinaturaIdCakto }, transaction: _session.Transaction);
        }

        public async Task<Assinatura?> ObterPorEmailComprador(string email)
        {
            // Rede de seguranca: venda avulsa (sem recorrencia) nao traz id de assinatura, entao o
            // e-mail do comprador e o unico elo entre o evento e a conta ja criada.
            var sql = @"SELECT TOP 1 * FROM Assinatura WHERE EmailComprador = @Email ORDER BY DataCriacao DESC";
            return await _session.Connection.QueryFirstOrDefaultAsync<Assinatura>(
                sql, new { Email = email }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Assinatura>> Listar()
        {
            var sql = @"SELECT * FROM Assinatura ORDER BY DataCriacao DESC";
            return await _session.Connection.QueryAsync<Assinatura>(sql, transaction: _session.Transaction);
        }

        public async Task<bool> EventoJaProcessado(string eventoIdCakto, string evento)
        {
            var sql = @"SELECT COUNT(1) FROM EventoCakto WHERE EventoIdCakto = @EventoIdCakto AND Evento = @Evento";
            var total = await _session.Connection.ExecuteScalarAsync<int>(
                sql, new { EventoIdCakto = eventoIdCakto, Evento = evento }, transaction: _session.Transaction);

            return total > 0;
        }

        public async Task RegistrarEvento(EventoCakto evento)
        {
            var sql = @"
                INSERT INTO EventoCakto (Id, EventoIdCakto, Evento, EmpresaId, PayloadJson, DataRecebido)
                VALUES (@Id, @EventoIdCakto, @Evento, @EmpresaId, @PayloadJson, @DataRecebido)";

            await _session.Connection.ExecuteAsync(sql, evento, transaction: _session.Transaction);
        }
    }
}
