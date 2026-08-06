using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data
{
    public class DbSession : IDisposable
    {
        private readonly IDbConnection _connection;
        public IDbTransaction Transaction;
        private readonly IConfiguration _configuration;

        public IDbConnection Connection => _connection;

        public DbSession(IConfiguration configuration)
        {
            _configuration = configuration;
            #if DEBUG
            var connectionSQLServer = configuration.GetConnectionString("ContactSolutionDB");
#else
            var connectionSQLServer = configuration.GetConnectionString("ContactProdDB");
#endif
            _connection = new SqlConnection(connectionSQLServer);
            _connection.Open();

        }

        public void Dispose()
        {
            _connection.Dispose();
        }

    }
}
