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
    public class ConversationsRepository : IConversationsRepository
    {

        private readonly DbSession _session;

        public ConversationsRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<int> Incluir(Conversations conversations)
        {
            var sql = $@"
                INSERT INTO {nameof(Conversations)} (
                    {nameof(Conversations.company_id)}, 
                    {nameof(Conversations.phone)}, 
                    {nameof(Conversations.status_funil)}, 
                    {nameof(Conversations.status)}, 
                    {nameof(Conversations.step)}, 
                    {nameof(Conversations.nome)}, 
                    {nameof(Conversations.email)}, 
                    {nameof(Conversations.updated_at)}
                ) 
                VALUES (
                    @CompanyId, 
                    @Phone, 
                    @StatusFunil, 
                    @Status, 
                    @Step, 
                    @Nome, 
                    @Email, 
                    @UpdatedAt
                ) 
                RETURNING {nameof(Conversations.id)}";

            var parameters = new DynamicParameters();
            parameters.Add("CompanyId", conversations.company_id);
            parameters.Add("Phone", conversations.phone);
            parameters.Add("StatusFunil", conversations.status_funil);
            parameters.Add("Status", conversations.status);
            parameters.Add("Step", conversations.step);
            parameters.Add("Nome", conversations.nome);
            parameters.Add("Email", conversations.email);
            parameters.Add("UpdatedAt", DateTimeOffset.UtcNow); // Usando UtcNow para consistência

            return await _session.Connection.QuerySingleAsync<int>(
                sql,
                parameters,
                transaction: _session.Transaction
            );
        }

        public async Task<List<Conversations>> Obter(string companyId)
        {
            var sql = $@"
                SELECT 
                    {nameof(Conversations.id)}, 
                    {nameof(Conversations.company_id)}, 
                    {nameof(Conversations.phone)}, 
                    {nameof(Conversations.status_funil)}, 
                    {nameof(Conversations.status)}, 
                    {nameof(Conversations.step)}, 
                    {nameof(Conversations.nome)}, 
                    {nameof(Conversations.email)}, 
                    {nameof(Conversations.updated_at)}
                FROM {nameof(Conversations)} 
                WHERE {nameof(Conversations.company_id)} = @CompanyId
                ORDER BY {nameof(Conversations.updated_at)} DESC";

            var parameters = new DynamicParameters();
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

