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
    public  class FlowsRepository : IFlowsRepository
    {
        private readonly DbSession _session;

        public FlowsRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<int> Incluir(Flows flow)
        {
            var sql = $@"
                INSERT INTO {nameof(Flows)} (
                    {nameof(Flows.id)}, 
                    {nameof(Flows.company_id)}, 
                    {nameof(Flows.name)}, 
                    {nameof(Flows.messages)}, 
                    {nameof(Flows.updated_at)}
                ) 
                VALUES (
                    @id, 
                    @company_id, 
                    @name, 
                    @messages, 
                    @updated_at
                )";

            // Como o 'id' do Flow parece ser uma string (GUID ou UID vindo do front),
            // geralmente o ExecuteAsync é suficiente. 
            // Se o ID for gerado pelo banco, use o padrão RETURNING que fizemos em Companies.
            return await _session.Connection.ExecuteAsync(
                sql,
                flow,
                transaction: _session.Transaction
            );
        }

        public async Task<List<Flows>> Obtem(string companyId)
        {
            var sql = $@"
                SELECT 
                    {nameof(Flows.id)}, 
                    {nameof(Flows.company_id)}, 
                    {nameof(Flows.name)}, 
                    {nameof(Flows.messages)}, 
                    {nameof(Flows.updated_at)}
                FROM {nameof(Flows)}
                WHERE {nameof(Flows.company_id)} = @CompanyId
                ORDER BY {nameof(Flows.updated_at)} DESC";

            var parameters = new DynamicParameters();
            parameters.Add("CompanyId", companyId);

            var result = await _session.Connection.QueryAsync<Flows>(
                sql,
                parameters,
                transaction: _session.Transaction
            );

            return result.ToList();
        }
    }
}

