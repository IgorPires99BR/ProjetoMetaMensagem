using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ContatoRepository : IContatoRepository
    {
        private readonly DbSession _session;

        public ContatoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Contato contato)
        {
            // Grava o Id gerado pela aplicacao em vez de deixar o DEFAULT (newid()) da coluna
            // decidir: antes o Contato saia daqui com um Guid que nao existia no banco, porque
            // o INSERT omitia a coluna e o banco criava outro. Ninguem usava esse Id ainda, mas
            // era uma armadilha pra primeira tela que precisasse do contato recem-criado.
            //
            // O SELECT SCOPE_IDENTITY() que existia aqui era resto de outro modelo: Contato.Id e
            // uniqueidentifier e nao identity, entao sempre devolvia NULL.
            var sql = $@"
                INSERT INTO {nameof(Contato)} (
                    {nameof(Contato.Id)},
                    {nameof(Contato.UsuarioId)},
                    {nameof(Contato.EmpresaId)},
                    {nameof(Contato.Telefone)},
                    {nameof(Contato.NomeContato)},
                    {nameof(Contato.Email)},
                    {nameof(Contato.NomeCliente)},
                    {nameof(Contato.DiaVencimento)},
                    {nameof(Contato.TaxaJuros)},
                    {nameof(Contato.TaxaJurosMensal)},
                    {nameof(Contato.ValorFatura)},
                    {nameof(Contato.DataCriacao)}
                )
                VALUES (
                    @Id, @UsuarioId, @EmpresaId, @Telefone, @NomeContato, @Email, @NomeCliente,
                    @DiaVencimento, @TaxaJuros, @TaxaJurosMensal, @ValorFatura, @DataCriacao
                );";

            var parameters = new
            {
                contato.Id,
                contato.UsuarioId,
                contato.EmpresaId,
                contato.Telefone,
                contato.NomeContato,
                contato.Email,
                contato.NomeCliente,
                contato.DiaVencimento,
                contato.TaxaJuros,
                contato.TaxaJurosMensal,
                contato.ValorFatura,
                DataCriacao = DateTimeOffset.Now
            };

            await _session.Connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar contato de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Alterar(Contato contato, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE {nameof(Contato)}
                SET
                    {nameof(Contato.Telefone)} = @Telefone,
                    {nameof(Contato.NomeContato)} = @NomeContato,
                    {nameof(Contato.Email)} = @Email,
                    {nameof(Contato.NomeCliente)} = @NomeCliente,
                    {nameof(Contato.DiaVencimento)} = @DiaVencimento,
                    {nameof(Contato.TaxaJuros)} = @TaxaJuros,
                    {nameof(Contato.TaxaJurosMensal)} = @TaxaJurosMensal,
                    {nameof(Contato.ValorFatura)} = @ValorFatura
                WHERE {nameof(Contato.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    contato.Id,
                    contato.Telefone,
                    contato.NomeContato,
                    contato.Email,
                    contato.NomeCliente,
                    contato.DiaVencimento,
                    contato.TaxaJuros,
                    contato.TaxaJurosMensal,
                    contato.ValorFatura,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(string id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Contato)}
                WHERE {nameof(Contato.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Contato?> ObterPorTelefone(Guid empresaId, string telefone)
        {
            // Normaliza os dois lados pra digitos apenas, ja que a Meta manda o "from" sempre
            // sem "+"/espacos, mas o cadastro manual do Contato pode ter formatacao.
            //
            // Varios contatos podem dividir o mesmo numero (uma pessoa que responde por varias
            // empresas do cliente). Antes o TOP 1 implicito devolvia um deles sem criterio, e a
            // resposta do cliente caia num contato qualquer. Agora vence, nesta ordem:
            //  1. o contato com conversa de flow em andamento (nao se perde o fio do bot no meio
            //     do atendimento so porque outro contato do mesmo numero recebeu uma cobranca);
            //  2. o contato pra quem mais recentemente ENVIAMOS algo -- e a esse envio que o
            //     cliente esta respondendo;
            //  3. o cadastro mais antigo, so pra o resultado ser sempre o mesmo.
            var sql = @"
        SELECT TOP 1 c.* FROM Contato c
        WHERE c.EmpresaId = @EmpresaId
          AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(c.Telefone, '+', ''), ' ', ''), '-', ''), '(', ''), ')', '') = @Telefone
        ORDER BY
          CASE WHEN EXISTS (SELECT 1 FROM EstadoConversa e
                            WHERE e.EmpresaId = c.EmpresaId AND e.ContatoId = c.Id AND e.Finalizado = 0)
               THEN 0 ELSE 1 END,
          (SELECT MAX(h.DataEnvio) FROM HistoricoDisparo h
           WHERE h.EmpresaId = c.EmpresaId AND h.ContatoId = c.Id) DESC,
          c.DataCriacao ASC,
          c.Id";

            var telefoneNormalizado = new string(telefone.Where(char.IsDigit).ToArray());

            return await _session.Connection.QueryFirstOrDefaultAsync<Contato>(
                sql,
                new { EmpresaId = empresaId, Telefone = telefoneNormalizado },
                transaction: _session.Transaction
            );
        }

        public async Task<IEnumerable<Contato>> ObterPorEmpresa(Guid? empresaId)
        {
            // null = conta de plataforma, ve contatos de todas as empresas de uma vez.
            var sql = @"
                SELECT * FROM Contato
                WHERE (@EmpresaId IS NULL OR EmpresaId = @EmpresaId)";

            return await _session.Connection.QueryAsync<Contato>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Contato>> ObterPorIds(Guid empresaId, IEnumerable<Guid> ids)
        {
            var idsLista = ids.Distinct().ToList();
            if (idsLista.Count == 0) return Enumerable.Empty<Contato>();

            var sql = @"
                SELECT * FROM Contato
                WHERE EmpresaId = @EmpresaId AND Id IN @Ids";

            return await _session.Connection.QueryAsync<Contato>(
                sql,
                new { EmpresaId = empresaId, Ids = idsLista },
                transaction: _session.Transaction);
        }
    }
}
