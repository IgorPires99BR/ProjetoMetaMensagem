using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly DbSession _session;

        public TemplateRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Template template)
        {
            var sql = $@"
                INSERT INTO {nameof(Template)} (
                    {nameof(Template.EmpresaId)},
                    {nameof(Template.NomeTemplate)},
                    {nameof(Template.NomeExibicao)},
                    {nameof(Template.Conteudo)},
                    {nameof(Template.Categoria)},
                    {nameof(Template.ComponentesJson)},
                    {nameof(Template.Idioma)},
                    {nameof(Template.Status)},
                    {nameof(Template.MetaTemplateId)},
                    {nameof(Template.GeraCobranca)}
                )
                VALUES (@EmpresaId, @NomeTemplate, @NomeExibicao, @Conteudo, @Categoria,@ComponentesJson, @Idioma, @Status, @MetaTemplateId, @GeraCobranca);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            await _session.Connection.ExecuteAsync(sql, template, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar template de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Alterar(Template template, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Template)}
                SET
                    {nameof(Template.NomeTemplate)} = @NomeTemplate,
                    {nameof(Template.NomeExibicao)} = @NomeExibicao,
                    {nameof(Template.Conteudo)} = @Conteudo,
                    {nameof(Template.Categoria)} = @Categoria,
                    {nameof(Template.ComponentesJson)} = @ComponentesJson,
                    {nameof(Template.Status)} = @Status,
                    {nameof(Template.MetaTemplateId)} = @MetaTemplateId
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    template.Id,
                    template.NomeTemplate,
                    template.NomeExibicao,
                    template.Conteudo,
                    template.Categoria,
                    template.ComponentesJson,
                    template.Status,
                    template.MetaTemplateId,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Template)}
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Template?> ObterPorId(int id)
        {
            var sql = $"SELECT * FROM {nameof(Template)} WHERE {nameof(Template.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Template>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Template?> ObterPorIdEEmpresa(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                SELECT * FROM {nameof(Template)}
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.QueryFirstOrDefaultAsync<Template>(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> Obter()
        {
            return await _session.Connection.QueryAsync<Template>($"SELECT * FROM {nameof(Template)}", transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> ObterPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(Template)} WHERE {nameof(Template.EmpresaId)} = @EmpresaId ORDER BY COALESCE({nameof(Template.NomeExibicao)}, {nameof(Template.NomeTemplate)})";
            return await _session.Connection.QueryAsync<Template>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Template>> ObterDeOutrasEmpresasDoWaba(Guid empresaId, string wabaId)
        {
            var sql = $@"
                SELECT t.* FROM {nameof(Template)} t
                INNER JOIN Empresa e ON e.Id = t.{nameof(Template.EmpresaId)}
                WHERE e.WabaId = @WabaId
                  AND t.{nameof(Template.EmpresaId)} <> @EmpresaId";

            return await _session.Connection.QueryAsync<Template>(sql,
                new { EmpresaId = empresaId, WabaId = wabaId },
                transaction: _session.Transaction);
        }

        public async Task<int> AlterarFlagCobranca(Guid id, Guid? empresaIdSolicitante, bool geraCobranca)
        {
            var sql = $@"
                UPDATE {nameof(Template)}
                SET {nameof(Template.GeraCobranca)} = @GeraCobranca
                WHERE {nameof(Template.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, GeraCobranca = geraCobranca, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<IEnumerable<TemplateGatilho>> ObterGatilhosPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(TemplateGatilho)} WHERE {nameof(TemplateGatilho.EmpresaId)} = @EmpresaId";
            return await _session.Connection.QueryAsync<TemplateGatilho>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<TemplateGatilho>> ObterGatilhosAtivos(Guid empresaId, string tipo)
        {
            var sql = $@"
                SELECT * FROM {nameof(TemplateGatilho)}
                WHERE {nameof(TemplateGatilho.EmpresaId)} = @EmpresaId
                  AND {nameof(TemplateGatilho.Tipo)} = @Tipo
                  AND {nameof(TemplateGatilho.Ativo)} = 1";

            return await _session.Connection.QueryAsync<TemplateGatilho>(sql,
                new { EmpresaId = empresaId, Tipo = tipo }, transaction: _session.Transaction);
        }

        public async Task SubstituirGatilhos(Guid templateId, Guid empresaId, IEnumerable<TemplateGatilho> gatilhos)
        {
            // Apagar e regravar perderia o historico de avisos de atraso (TemplateGatilhoEnvio
            // cai em cascata) e o mesmo atraso seria avisado de novo. Por isso o que ja existe
            // e atualizado no lugar e so o tipo que saiu da lista e apagado.
            var existentes = (await _session.Connection.QueryAsync<TemplateGatilho>(
                $"SELECT * FROM {nameof(TemplateGatilho)} WHERE {nameof(TemplateGatilho.TemplateId)} = @TemplateId",
                new { TemplateId = templateId }, transaction: _session.Transaction)).ToList();

            var novos = gatilhos.ToList();

            foreach (var removido in existentes.Where(e => novos.All(n => n.Tipo != e.Tipo)))
            {
                await _session.Connection.ExecuteAsync(
                    $"DELETE FROM {nameof(TemplateGatilho)} WHERE {nameof(TemplateGatilho.Id)} = @Id",
                    new { removido.Id }, transaction: _session.Transaction);
            }

            foreach (var gatilho in novos)
            {
                var existente = existentes.FirstOrDefault(e => e.Tipo == gatilho.Tipo);
                if (existente != null)
                {
                    await _session.Connection.ExecuteAsync($@"
                        UPDATE {nameof(TemplateGatilho)}
                        SET {nameof(TemplateGatilho.DiasAtraso)} = @DiasAtraso,
                            {nameof(TemplateGatilho.VariaveisJson)} = @VariaveisJson,
                            {nameof(TemplateGatilho.Ativo)} = @Ativo,
                            {nameof(TemplateGatilho.DataAtualizacao)} = GETDATE()
                        WHERE {nameof(TemplateGatilho.Id)} = @Id",
                        new { existente.Id, gatilho.DiasAtraso, gatilho.VariaveisJson, gatilho.Ativo },
                        transaction: _session.Transaction);
                    continue;
                }

                await _session.Connection.ExecuteAsync($@"
                    INSERT INTO {nameof(TemplateGatilho)} (
                        {nameof(TemplateGatilho.Id)}, {nameof(TemplateGatilho.EmpresaId)}, {nameof(TemplateGatilho.TemplateId)},
                        {nameof(TemplateGatilho.Tipo)}, {nameof(TemplateGatilho.DiasAtraso)}, {nameof(TemplateGatilho.VariaveisJson)},
                        {nameof(TemplateGatilho.Ativo)}, {nameof(TemplateGatilho.DataCriacao)})
                    VALUES (@Id, @EmpresaId, @TemplateId, @Tipo, @DiasAtraso, @VariaveisJson, @Ativo, @DataCriacao)",
                    new
                    {
                        gatilho.Id,
                        EmpresaId = empresaId,
                        TemplateId = templateId,
                        gatilho.Tipo,
                        gatilho.DiasAtraso,
                        gatilho.VariaveisJson,
                        gatilho.Ativo,
                        gatilho.DataCriacao
                    },
                    transaction: _session.Transaction);
            }
        }

        public async Task<IEnumerable<CobrancaEmAtraso>> ObterCobrancasEmAtrasoSemAviso(DateTime hoje)
        {
            // Cobranca aberta por um disparo do proprio gatilho fica de fora: um aviso de atraso
            // com "gera cobranca" abriria outra cobranca, que atrasaria e geraria outro aviso,
            // sem fim.
            var sql = $@"
                SELECT g.Id AS GatilhoId, g.EmpresaId, c.Id AS CobrancaClienteId, c.ContatoId
                FROM {nameof(TemplateGatilho)} g
                INNER JOIN CobrancaCliente c ON c.EmpresaId = g.EmpresaId
                LEFT JOIN HistoricoDisparo h ON h.Id = c.HistoricoDisparoId
                WHERE g.Tipo = @Tipo
                  AND g.Ativo = 1
                  AND c.Status IN (@Pendente, @Vencida)
                  AND DATEADD(DAY, ISNULL(g.DiasAtraso, 0), CAST(c.DataVencimento AS DATE)) <= CAST(@Hoje AS DATE)
                  AND DATEADD(DAY, ISNULL(g.DiasAtraso, 0), CAST(c.DataVencimento AS DATE)) >= CAST(g.DataCriacao AS DATE)
                  AND ISNULL(h.Origem, '') <> @OrigemGatilho
                  AND NOT EXISTS (
                      SELECT 1 FROM TemplateGatilhoEnvio e
                      WHERE e.GatilhoId = g.Id AND e.CobrancaClienteId = c.Id)";

            return await _session.Connection.QueryAsync<CobrancaEmAtraso>(sql,
                new
                {
                    Tipo = TemplateGatilho.AtrasoPagamento,
                    Pendente = StatusCobrancaCliente.Pendente,
                    Vencida = StatusCobrancaCliente.Vencida,
                    Hoje = hoje,
                    OrigemGatilho = ProjetoMetaMensagem.Dominio.Common.OrigemDisparo.GatilhoTemplate
                },
                transaction: _session.Transaction);
        }

        public async Task RegistrarEnvioGatilho(Guid gatilhoId, Guid cobrancaClienteId)
        {
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM TemplateGatilhoEnvio WHERE GatilhoId = @GatilhoId AND CobrancaClienteId = @CobrancaClienteId)
                    INSERT INTO TemplateGatilhoEnvio (GatilhoId, CobrancaClienteId) VALUES (@GatilhoId, @CobrancaClienteId)";

            await _session.Connection.ExecuteAsync(sql,
                new { GatilhoId = gatilhoId, CobrancaClienteId = cobrancaClienteId },
                transaction: _session.Transaction);
        }
    }
}

