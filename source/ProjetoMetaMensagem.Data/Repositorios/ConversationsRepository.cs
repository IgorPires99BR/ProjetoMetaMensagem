using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    // Lista os leads da empresa (endpoint /api/leads).
    //
    // A versao anterior consultava uma tabela `Conversations` que NUNCA existiu neste banco --
    // era resquicio do modelo antigo em Postgres/Supabase (o INSERT ainda usava `RETURNING`,
    // sintaxe que o SQL Server nem aceita). Resultado: o endpoint respondia 500 em qualquer
    // ambiente desde sempre. Aqui ele passa a ler o que de fato representa um lead hoje:
    // o Contato da empresa, com o estado da conversa e a etapa do funil quando existirem.
    public class ConversationsRepository : IConversationsRepository
    {
        private readonly DbSession _session;

        public ConversationsRepository(DbSession session)
        {
            _session = session;
        }

        public Task<int> Incluir(Conversations conversations)
        {
            // Lead nao e criado por aqui: ele nasce como Contato (api/contato/incluir) ou entra
            // no funil pelo CRM (api/pipeline/lead/adicionar).
            throw new NotSupportedException(
                "Leads não são incluídos por este caminho. Use api/contato/incluir ou api/pipeline/lead/adicionar.");
        }

        public async Task<List<Conversations>> Obter(string companyId)
        {
            if (!Guid.TryParse(companyId, out var empresaId))
                return new List<Conversations>();

            // Contato nao tem EmpresaId: o vinculo e via Usuario, como no DashboardRepository.
            var sql = @"
                SELECT
                    c.Id            AS id,
                    @CompanyId      AS company_id,
                    c.Telefone      AS phone,
                    pe.Nome         AS status_funil,
                    CASE
                        WHEN ec.AssumidoPorUsuarioId IS NOT NULL THEN 'Atendimento humano'
                        WHEN ec.Id IS NOT NULL AND ec.Finalizado = 0 THEN 'Em conversa'
                        WHEN ec.Id IS NOT NULL THEN 'Conversa finalizada'
                        ELSE 'Sem conversa'
                    END             AS status,
                    ec.EtapaAtualId AS step,
                    c.Nome          AS nome,
                    c.Email         AS email,
                    COALESCE(ec.DataAtualizacao, c.DataAtualizacao, c.DataCriacao) AS updated_at
                FROM Contato c
                INNER JOIN Usuario u ON u.Id = c.UsuarioId
                OUTER APPLY (
                    SELECT TOP 1 e.Id, e.Finalizado, e.AssumidoPorUsuarioId, e.EtapaAtualId, e.DataAtualizacao
                    FROM EstadoConversa e
                    WHERE e.ContatoId = c.Id AND e.EmpresaId = @EmpresaId
                    ORDER BY e.DataAtualizacao DESC
                ) ec
                OUTER APPLY (
                    SELECT TOP 1 et.Nome
                    FROM LeadPipeline lp
                    INNER JOIN PipelineEtapa et ON et.Id = lp.PipelineEtapaId
                    WHERE lp.ContatoId = c.Id AND lp.EmpresaId = @EmpresaId
                    ORDER BY lp.DataUltimaAlteracao DESC
                ) pe
                WHERE u.EmpresaId = @EmpresaId
                ORDER BY updated_at DESC";

            var parameters = new DynamicParameters();
            parameters.Add("EmpresaId", empresaId);
            parameters.Add("CompanyId", companyId);

            var result = await _session.Connection.QueryAsync<Conversations>(
                sql,
                parameters,
                transaction: _session.Transaction
            );

            return result.ToList();
        }
    }
}
