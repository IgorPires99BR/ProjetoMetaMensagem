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
        public IDbConnection _connection;
        public IDbTransaction Transaction;
        public IConfiguration _configuration;

        public DbSession(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionSQLServer = configuration.GetConnectionString("ContactSolutionDB");
            _connection = new SqlConnection(connectionSQLServer);
            _connection.Open();

        }

        public void Dispose()
        {
            _connection.Dispose();
        }

    }
}
