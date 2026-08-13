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

            await _session.Connection.ExecuteAsync(sql, campanha, transaction: _session.Transaction);
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
                await _session.Connection.ExecuteAsync(sql, contato, transaction: _session.Transaction);
            }
        }

        public async Task<IEnumerable<Campanha>> Listar(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(Campanha.DataCriacao)} DESC;";

            return await _session.Connection.QueryAsync<Campanha>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Campanha>> ObterPendentes()
        {
            // PROCESSANDO entra junto de AGENDADA: quando o processo caia no meio (deploy,
            // restart), a campanha ficava marcada assim pra sempre e ninguem a retomava. Nao
            // ha risco de reenvio porque quem decide o que enviar e o vinculo, nao o status --
            // o worker so trata contato com Processado = 0. CANCELADA fica de fora de proposito.
            //
            // Sem filtro por vinculo pendente: uma campanha que ja tratou todo mundo mas ficou
            // em PROCESSANDO precisa ser coletada mais uma vez justamente pra virar CONCLUIDA e
            // sair da fila. Como a passada seguinte a conclui, ela nao volta.
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.Status)} IN ('AGENDADA', 'PROCESSANDO')
                  AND {nameof(Campanha.DataAgendamento)} <= @Agora
                ORDER BY {nameof(Campanha.DataAgendamento)};";

            return await _session.Connection.QueryAsync<Campanha>(
                sql, new { Agora = DateTime.Now }, transaction: _session.Transaction);
        }

        public async Task<bool> ReivindicarContato(Guid vinculoId)
        {
            // Marca o contato como tratado ANTES do envio, condicionado a ele ainda estar
            // pendente. O WHERE Processado = 0 e o que torna a operacao atomica: se dois
            // workers rodarem ao mesmo tempo (acontece durante um deploy), so um afeta linha
            // e o outro pula, entao o contato nao recebe a mensagem duas vezes.
            var sql = $@"
                UPDATE {nameof(CampanhaContato)}
                SET {nameof(CampanhaContato.Processado)} = 1,
                    {nameof(CampanhaContato.Sucesso)} = 0,
                    {nameof(CampanhaContato.MensagemErro)} = @Motivo
                WHERE {nameof(CampanhaContato.Id)} = @Id
                  AND {nameof(CampanhaContato.Processado)} = 0;";

            var linhas = await _session.Connection.ExecuteAsync(sql,
                new { Id = vinculoId, Motivo = CampanhaContato.EnvioInterrompido },
                transaction: _session.Transaction);

            return linhas == 1;
        }

        public async Task AtualizarResultadoContato(CampanhaContato vinculo)
        {
            // Gravado a cada contato, e nao em bloco no fim: se o worker cair ou o processo for
            // reiniciado no meio de uma campanha grande, o que ja foi enviado fica registrado.
            var sql = $@"
                UPDATE {nameof(CampanhaContato)}
                SET {nameof(CampanhaContato.Processado)} = @{nameof(CampanhaContato.Processado)},
                    {nameof(CampanhaContato.Sucesso)} = @{nameof(CampanhaContato.Sucesso)},
                    {nameof(CampanhaContato.MensagemErro)} = @{nameof(CampanhaContato.MensagemErro)}
                WHERE {nameof(CampanhaContato.Id)} = @{nameof(CampanhaContato.Id)};";

            await _session.Connection.ExecuteAsync(sql, vinculo, transaction: _session.Transaction);
        }

        public async Task AtualizarProgresso(Guid campanhaId, int processados)
        {
            var sql = $@"
                UPDATE {nameof(Campanha)}
                SET {nameof(Campanha.Processados)} = @Processados
                WHERE {nameof(Campanha.Id)} = @Id;";

            await _session.Connection.ExecuteAsync(sql,
                new { Id = campanhaId, Processados = processados }, transaction: _session.Transaction);
        }

        public async Task<int> AtualizarStatus(Guid id, string status, Guid? empresaIdSolicitante)
        {
            // Recorte de empresa no WHERE: antes o UPDATE casava so pelo Id e qualquer usuario
            // logado cancelava a campanha de outra empresa mandando o id na rota.
            var sql = $@"
                UPDATE {nameof(Campanha)}
                SET {nameof(Campanha.Status)} = @Status
                WHERE {nameof(Campanha.Id)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(Campanha.EmpresaId)} = @EmpresaIdSolicitante);";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, Status = status, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Campanha?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Campanha)}
                WHERE {nameof(Campanha.Id)} = @Id;";

            return await _session.Connection.QueryFirstOrDefaultAsync<Campanha>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<CampanhaContato>> ObterContatosPorCampanha(Guid campanhaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(CampanhaContato)}
                WHERE {nameof(CampanhaContato.CampanhaId)} = @CampanhaId;";

            return await _session.Connection.QueryAsync<CampanhaContato>(
                sql, new { CampanhaId = campanhaId }, transaction: _session.Transaction);
        }
    }
}

