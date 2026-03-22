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
            throw new NotImplementedException();
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
    }
}
