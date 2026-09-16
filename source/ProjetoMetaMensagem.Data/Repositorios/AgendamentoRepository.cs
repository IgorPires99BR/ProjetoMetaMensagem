using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class AgendamentoRepository : IAgendamentoRepository
    {
        private readonly DbSession _session;

        public AgendamentoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<Guid> Incluir(Agendamento agendamento)
        {
            var sql = $@"
                INSERT INTO {nameof(Agendamento)} (
                    {nameof(Agendamento.Id)},
                    {nameof(Agendamento.EmpresaId)},
                    {nameof(Agendamento.Nome)},
                    {nameof(Agendamento.TemplateId)},
                    {nameof(Agendamento.TipoRecorrencia)},
                    {nameof(Agendamento.DataInicio)},
                    {nameof(Agendamento.DataFim)},
                    {nameof(Agendamento.DataReferencia)},
                    {nameof(Agendamento.ProximaExecucao)},
                    {nameof(Agendamento.Ativo)},
                    {nameof(Agendamento.UsuarioCriacaoId)},
                    {nameof(Agendamento.DataCriacao)},
                    {nameof(Agendamento.VariaveisJson)},
                    {nameof(Agendamento.DiasSemana)},
                    {nameof(Agendamento.DiaDoMes)}
                ) VALUES (
                    @{nameof(Agendamento.Id)},
                    @{nameof(Agendamento.EmpresaId)},
                    @{nameof(Agendamento.Nome)},
                    @{nameof(Agendamento.TemplateId)},
                    @{nameof(Agendamento.TipoRecorrencia)},
                    @{nameof(Agendamento.DataInicio)},
                    @{nameof(Agendamento.DataFim)},
                    @{nameof(Agendamento.DataReferencia)},
                    @{nameof(Agendamento.ProximaExecucao)},
                    @{nameof(Agendamento.Ativo)},
                    @{nameof(Agendamento.UsuarioCriacaoId)},
                    @{nameof(Agendamento.DataCriacao)},
                    @{nameof(Agendamento.VariaveisJson)},
                    @{nameof(Agendamento.DiasSemana)},
                    @{nameof(Agendamento.DiaDoMes)}
                );";

            await _session.Connection.ExecuteAsync(sql, agendamento, transaction: _session.Transaction);
            return agendamento.Id;
        }

        public async Task IncluirContatos(List<AgendamentoContato> contatos)
        {
            if (contatos == null || contatos.Count == 0) return;

            var sql = $@"
                INSERT INTO {nameof(AgendamentoContato)} (
                    {nameof(AgendamentoContato.Id)},
                    {nameof(AgendamentoContato.AgendamentoId)},
                    {nameof(AgendamentoContato.ContatoId)}
                ) VALUES (
                    @{nameof(AgendamentoContato.Id)},
                    @{nameof(AgendamentoContato.AgendamentoId)},
                    @{nameof(AgendamentoContato.ContatoId)}
                );";

            await _session.Connection.ExecuteAsync(sql, contatos, transaction: _session.Transaction);
        }

        public async Task SubstituirContatos(Guid agendamentoId, List<Guid> contatoIds)
        {
            var sqlExcluir = $@"
                DELETE FROM {nameof(AgendamentoContato)}
                WHERE {nameof(AgendamentoContato.AgendamentoId)} = @AgendamentoId;";

            await _session.Connection.ExecuteAsync(sqlExcluir,
                new { AgendamentoId = agendamentoId }, transaction: _session.Transaction);

            var contatos = (contatoIds ?? new List<Guid>())
                .Select(contatoId => new AgendamentoContato
                {
                    Id = Guid.NewGuid(),
                    AgendamentoId = agendamentoId,
                    ContatoId = contatoId
                })
                .ToList();

            await IncluirContatos(contatos);
        }

        public async Task<IEnumerable<AgendamentoListItem>> Listar(Guid empresaId)
        {
            var sql = $@"
                SELECT
                    a.{nameof(Agendamento.Id)} AS {nameof(AgendamentoListItem.Id)},
                    a.{nameof(Agendamento.Nome)} AS {nameof(AgendamentoListItem.Nome)},
                    a.{nameof(Agendamento.TemplateId)} AS {nameof(AgendamentoListItem.TemplateId)},
                    t.{nameof(Template.NomeTemplate)} AS {nameof(AgendamentoListItem.NomeTemplate)},
                    a.{nameof(Agendamento.TipoRecorrencia)} AS {nameof(AgendamentoListItem.TipoRecorrencia)},
                    a.{nameof(Agendamento.DataInicio)} AS {nameof(AgendamentoListItem.DataInicio)},
                    a.{nameof(Agendamento.DataFim)} AS {nameof(AgendamentoListItem.DataFim)},
                    a.{nameof(Agendamento.ProximaExecucao)} AS {nameof(AgendamentoListItem.ProximaExecucao)},
                    a.{nameof(Agendamento.Ativo)} AS {nameof(AgendamentoListItem.Ativo)},
                    a.{nameof(Agendamento.DiasSemana)} AS {nameof(AgendamentoListItem.DiasSemana)},
                    a.{nameof(Agendamento.DiaDoMes)} AS {nameof(AgendamentoListItem.DiaDoMes)},
                    (SELECT COUNT(*) FROM {nameof(AgendamentoContato)} ac
                        WHERE ac.{nameof(AgendamentoContato.AgendamentoId)} = a.{nameof(Agendamento.Id)})
                        AS {nameof(AgendamentoListItem.TotalContatos)}
                FROM {nameof(Agendamento)} a
                LEFT JOIN {nameof(Template)} t ON t.{nameof(Template.Id)} = a.{nameof(Agendamento.TemplateId)}
                WHERE a.{nameof(Agendamento.EmpresaId)} = @EmpresaId
                ORDER BY a.{nameof(Agendamento.DataCriacao)} DESC;";

            return await _session.Connection.QueryAsync<AgendamentoListItem>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<Agendamento?> ObterPorId(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                SELECT * FROM {nameof(Agendamento)}
                WHERE {nameof(Agendamento.Id)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(Agendamento.EmpresaId)} = @EmpresaIdSolicitante);";

            return await _session.Connection.QueryFirstOrDefaultAsync<Agendamento>(
                sql, new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Guid>> ObterContatoIds(Guid agendamentoId)
        {
            var sql = $@"
                SELECT {nameof(AgendamentoContato.ContatoId)} FROM {nameof(AgendamentoContato)}
                WHERE {nameof(AgendamentoContato.AgendamentoId)} = @AgendamentoId;";

            return await _session.Connection.QueryAsync<Guid>(
                sql, new { AgendamentoId = agendamentoId }, transaction: _session.Transaction);
        }

        public async Task<int> Atualizar(Agendamento agendamento, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Agendamento)}
                SET {nameof(Agendamento.Nome)} = @{nameof(Agendamento.Nome)},
                    {nameof(Agendamento.TemplateId)} = @{nameof(Agendamento.TemplateId)},
                    {nameof(Agendamento.TipoRecorrencia)} = @{nameof(Agendamento.TipoRecorrencia)},
                    {nameof(Agendamento.DataInicio)} = @{nameof(Agendamento.DataInicio)},
                    {nameof(Agendamento.DataFim)} = @{nameof(Agendamento.DataFim)},
                    {nameof(Agendamento.DataReferencia)} = @{nameof(Agendamento.DataReferencia)},
                    {nameof(Agendamento.ProximaExecucao)} = @{nameof(Agendamento.ProximaExecucao)},
                    {nameof(Agendamento.DataAtualizacao)} = @{nameof(Agendamento.DataAtualizacao)},
                    {nameof(Agendamento.VariaveisJson)} = @{nameof(Agendamento.VariaveisJson)},
                    {nameof(Agendamento.DiasSemana)} = @{nameof(Agendamento.DiasSemana)},
                    {nameof(Agendamento.DiaDoMes)} = @{nameof(Agendamento.DiaDoMes)}
                WHERE {nameof(Agendamento.Id)} = @{nameof(Agendamento.Id)}
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(Agendamento.EmpresaId)} = @EmpresaIdSolicitante);";

            var parametros = new DynamicParameters(agendamento);
            parametros.Add("EmpresaIdSolicitante", empresaIdSolicitante);

            return await _session.Connection.ExecuteAsync(sql, parametros, transaction: _session.Transaction);
        }

        public async Task<int> AtualizarStatus(Guid id, bool ativo, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Agendamento)}
                SET {nameof(Agendamento.Ativo)} = @Ativo,
                    {nameof(Agendamento.DataAtualizacao)} = @Agora
                WHERE {nameof(Agendamento.Id)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(Agendamento.EmpresaId)} = @EmpresaIdSolicitante);";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, Ativo = ativo, Agora = DateTime.Now, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<int> Deletar(Guid id, Guid? empresaIdSolicitante)
        {
            // AgendamentoContato/AgendamentoExecucao caem junto via ON DELETE CASCADE.
            var sql = $@"
                DELETE FROM {nameof(Agendamento)}
                WHERE {nameof(Agendamento.Id)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(Agendamento.EmpresaId)} = @EmpresaIdSolicitante);";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Agendamento>> ObterPendentes(DateTime agora)
        {
            var sql = $@"
                SELECT * FROM {nameof(Agendamento)}
                WHERE {nameof(Agendamento.Ativo)} = 1
                  AND {nameof(Agendamento.ProximaExecucao)} <= @Agora
                  AND ({nameof(Agendamento.ProcessandoAte)} IS NULL OR {nameof(Agendamento.ProcessandoAte)} < @Agora)
                ORDER BY {nameof(Agendamento.ProximaExecucao)};";

            return await _session.Connection.QueryAsync<Agendamento>(
                sql, new { Agora = agora }, transaction: _session.Transaction);
        }

        public async Task<bool> ReivindicarAgendamento(Guid id, DateTime prazoProcessamento)
        {
            // Mesma logica de reserva por prazo do EstadoConversa.ProcessandoAte: sem lock de
            // banco, quem grava a reserva processa, quem nao consegue desiste na hora. Se o
            // processo cair no meio, a reserva expira sozinha e a proxima passada retoma.
            var sql = $@"
                UPDATE {nameof(Agendamento)}
                SET {nameof(Agendamento.ProcessandoAte)} = @Prazo
                WHERE {nameof(Agendamento.Id)} = @Id
                  AND ({nameof(Agendamento.ProcessandoAte)} IS NULL OR {nameof(Agendamento.ProcessandoAte)} < GETDATE());";

            var linhas = await _session.Connection.ExecuteAsync(sql,
                new { Id = id, Prazo = prazoProcessamento }, transaction: _session.Transaction);

            return linhas == 1;
        }

        public async Task FinalizarExecucao(Guid id, DateTime proximaExecucao, bool ativo)
        {
            var sql = $@"
                UPDATE {nameof(Agendamento)}
                SET {nameof(Agendamento.ProximaExecucao)} = @ProximaExecucao,
                    {nameof(Agendamento.Ativo)} = @Ativo,
                    {nameof(Agendamento.ProcessandoAte)} = NULL
                WHERE {nameof(Agendamento.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(sql,
                new { Id = id, ProximaExecucao = proximaExecucao, Ativo = ativo }, transaction: _session.Transaction);
        }

        public async Task<Guid> IncluirExecucao(AgendamentoExecucao execucao)
        {
            var sql = $@"
                INSERT INTO {nameof(AgendamentoExecucao)} (
                    {nameof(AgendamentoExecucao.Id)},
                    {nameof(AgendamentoExecucao.AgendamentoId)},
                    {nameof(AgendamentoExecucao.DataExecucao)},
                    {nameof(AgendamentoExecucao.TotalContatos)},
                    {nameof(AgendamentoExecucao.Sucessos)},
                    {nameof(AgendamentoExecucao.Falhas)},
                    {nameof(AgendamentoExecucao.Status)}
                ) VALUES (
                    @{nameof(AgendamentoExecucao.Id)},
                    @{nameof(AgendamentoExecucao.AgendamentoId)},
                    @{nameof(AgendamentoExecucao.DataExecucao)},
                    @{nameof(AgendamentoExecucao.TotalContatos)},
                    @{nameof(AgendamentoExecucao.Sucessos)},
                    @{nameof(AgendamentoExecucao.Falhas)},
                    @{nameof(AgendamentoExecucao.Status)}
                );";

            await _session.Connection.ExecuteAsync(sql, execucao, transaction: _session.Transaction);
            return execucao.Id;
        }

        public async Task<IEnumerable<AgendamentoExecucao>> ListarExecucoes(Guid agendamentoId, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                SELECT e.* FROM {nameof(AgendamentoExecucao)} e
                INNER JOIN {nameof(Agendamento)} a ON a.{nameof(Agendamento.Id)} = e.{nameof(AgendamentoExecucao.AgendamentoId)}
                WHERE e.{nameof(AgendamentoExecucao.AgendamentoId)} = @AgendamentoId
                  AND (@EmpresaIdSolicitante IS NULL
                       OR a.{nameof(Agendamento.EmpresaId)} = @EmpresaIdSolicitante)
                ORDER BY e.{nameof(AgendamentoExecucao.DataExecucao)} DESC;";

            return await _session.Connection.QueryAsync<AgendamentoExecucao>(
                sql, new { AgendamentoId = agendamentoId, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }
    }
}
