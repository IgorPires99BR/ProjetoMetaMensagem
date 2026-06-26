using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class CampanhaRepository : ICampanhaRepository
    {
        private readonly DbSession _session;

        public CampanhaRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<Guid> Incluir(Campanha campanha)
        {
            var sql = $@"
                INSERT INTO {nameof(Campanha)} (
                    {nameof(Campanha.Id)},
                    {nameof(Campanha.EmpresaId)},
                    {nameof(Campanha.Nome)},
                    {nameof(Campanha.TemplateId)},
                    {nameof(Campanha.ConteudoLivre)},
                    {nameof(Campanha.DataAgendamento)},
                    {nameof(Campanha.Status)},
                    {nameof(Campanha.TotalContatos)},
                    {nameof(Campanha.Processados)},
                    {nameof(Campanha.DataCriacao)}
                ) VALUES (
                    @{nameof(Campanha.Id)},
                    @{nameof(Campanha.EmpresaId)},
                    @{nameof(Campanha.Nome)},
                    @{nameof(Campanha.TemplateId)},
                    @{nameof(Campanha.ConteudoLivre)},
                    @{nameof(Campanha.DataAgendamento)},
                    @{nameof(Campanha.Status)},
                    @{nameof(Campanha.TotalContatos)},
                    @{nameof(Campanha.Processados)},
                    @{nameof(Campanha.DataCriacao)}
                );";

            await _session._connection.ExecuteAsync(sql, campanha, transaction: _session.Transaction);
            return campanha.Id;
        }

        public async Task IncluirContatos(List<CampanhaContato> contatos)
        {
            if (contatos == null || contatos.Count == 0) return;

            var sql = $@"
                INSERT INTO {nameof(CampanhaContato)} (
                    {nameof(CampanhaContato.Id)},
                    {nameof(CampanhaContato.CampanhaId)},
                    {nameof(CampanhaContato.ContatoId)},
                    {nameof(CampanhaContato.Processado)},
                    {nameof(CampanhaContato.Sucesso)},
                    {nameof(CampanhaContato.MensagemErro)}
                ) VALUES (
                    @{nameof(CampanhaContato.Id)},
                    @{nameof(CampanhaContato.CampanhaId)},
                    @{nameof(CampanhaContato.ContatoId)},
                    @{nameof(CampanhaContato.Processado)},
                    @{nameof(CampanhaContato.Sucesso)},
                    @{nameof(CampanhaContato.MensagemErro)}
                );";

            foreach (var contato in contatos)
            {
                await _session._connection.ExecuteAsync(sql, contato, transaction: _session.Transaction);
            }
        }

        public async Task<IEnumerable<Campanha>> Listar(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(Campanha.DataCriacao)} DESC;";

            return await _session._connection.QueryAsync<Campanha>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Campanha>> ObterPendentes()
        {
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.Status)} = 'AGENDADA'
                  AND {nameof(Campanha.DataAgendamento)} <= @Agora
                ORDER BY {nameof(Campanha.DataAgendamento)};";

            return await _session._connection.QueryAsync<Campanha>(
                sql, new { Agora = DateTime.Now }, transaction: _session.Transaction);
        }

        public async Task AtualizarStatus(Guid id, string status)
        {
            var sql = $@"
                UPDATE {nameof(Campanha)}
                SET {nameof(Campanha.Status)} = @Status
                WHERE {nameof(Campanha.Id)} = @Id;";

            await _session._connection.ExecuteAsync(sql,
                new { Id = id, Status = status }, transaction: _session.Transaction);
        }

        public async Task<Campanha?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.Id)} = @Id;";

            return await _session._connection.QueryFirstOrDefaultAsync<Campanha>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<CampanhaContato>> ObterContatosPorCampanha(Guid campanhaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(CampanhaContato)}
                WHERE {nameof(CampanhaContato.CampanhaId)} = @CampanhaId;";

            return await _session._connection.QueryAsync<CampanhaContato>(
                sql, new { CampanhaId = campanhaId }, transaction: _session.Transaction);
        }
    }
}
