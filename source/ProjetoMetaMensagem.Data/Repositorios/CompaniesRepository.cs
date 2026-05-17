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
    public class CompaniesRepository : ICompaniesRepository
    {
        private readonly DbSession _session;

        public CompaniesRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<int> Incluir(Companies empresa)
        {
            var sql = $@"
        INSERT INTO {nameof(Companies)} (
            {nameof(Companies.name)}, 
            {nameof(Companies.email)}, 
            {nameof(Companies.phone)}, 
            {nameof(Companies.bot_whatsapp)}, 
            {nameof(Companies.password)},
            {nameof(Companies.created_at)}
        ) 
        VALUES (
            @Nome, 
            @Email, 
            @Telefone, 
            @BotWhatsapp, 
            @Senha, 
            @CreatedAt
        ) 
        RETURNING {nameof(Companies.id)}"; // Retorna o ID gerado

            var parameters = new DynamicParameters();
            parameters.Add("Nome", empresa.name);
            parameters.Add("Email", empresa.email);
            parameters.Add("Telefone", empresa.phone);
            parameters.Add("BotWhatsapp", empresa.bot_whatsapp);
            parameters.Add("Senha", empresa.password);
            parameters.Add("CreatedAt", DateTime.Now);

            return await _session._connection.QuerySingleAsync<int>(
                sql,
                parameters,
                transaction: _session.Transaction
            );
        }

        public async Task<Companies> Login(string email, string senha)
        {
            var sql = $@"
        SELECT
            {nameof(Companies.id)},
            {nameof(Companies.bot_whatsapp)},
            {nameof(Companies.name)},
            {nameof(Companies.phone)},
            {nameof(Companies.created_at)}
        FROM
            {nameof(Companies)}

        WHERE {nameof(Companies.email)} = @{nameof(Companies.email)} 
          AND {nameof(Companies.password)} = @{nameof(Companies.password)}
                ";

            var parameters = new DynamicParameters();
            parameters.Add("email", email);
            parameters.Add("password", senha);

            // 3. Execução com QueryFirstOrDefaultAsync (retorna 1 ou null)
            var company = await _session._connection.QueryFirstOrDefaultAsync<Companies>(
                sql,
                parameters,
                transaction: _session.Transaction
            );

            return company;
        }

        public async Task Alterar(int companyId, Dominio.Entidades.Companies command)
        {
            var sql = $@"
                UPDATE {nameof(Companies)}
                SET 
                    {nameof(Companies.name)} = @Nome,
                    {nameof(Companies.email)} = @Email,
                    {nameof(Companies.phone)} = @Telefone,
                    {nameof(Companies.bot_whatsapp)} = @BotWhatsapp
                WHERE 
                    {nameof(Companies.id)} = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", companyId);
            parameters.Add("Nome", command.name);
            parameters.Add("Email", command.email);
            parameters.Add("Telefone", command.phone);
            parameters.Add("BotWhatsapp", command.bot_whatsapp);

            await _session._connection.ExecuteAsync(
                sql,
                parameters,
                transaction: _session.Transaction
            );
        }

        public async Task Deletar(int companyId)
        {
            var sql = $@"
                DELETE FROM {nameof(Companies)} 
                WHERE {nameof(Companies.id)} = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", companyId);

            await _session._connection.ExecuteAsync(
                sql,
                parameters,
                transaction: _session.Transaction
            );
        }

        public async Task<List<Companies>> Obter()
        {
            var sql = $@"
        SELECT 
            {nameof(Companies.id)}, 
            {nameof(Companies.name)}, 
            {nameof(Companies.email)}, 
            {nameof(Companies.phone)}, 
            {nameof(Companies.bot_whatsapp)}, 
            {nameof(Companies.created_at)} 
        FROM {nameof(Companies)} 
        ORDER BY {nameof(Companies.created_at)} DESC";


            var result = await _session._connection.QueryAsync<Companies>(
                sql,
                transaction: _session.Transaction
            );

            return result.ToList();
        }
    }
}
