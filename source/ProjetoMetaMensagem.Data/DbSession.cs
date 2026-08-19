using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
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

        // Conexao separada, fora da transacao da requisicao. Usada pela reserva de processamento
        // do Flow: ela precisa ser enxergada pelas OUTRAS requisicoes na hora, e escrita dentro
        // da transacao aberta so aparece depois do commit. Quem chama e responsavel por fechar.
        public IDbConnection AbrirConexaoIndependente()
        {
            #if DEBUG
            var conexao = _configuration.GetConnectionString("ContactSolutionDB");
#else
            var conexao = _configuration.GetConnectionString("ContactProdDB");
#endif
            var nova = new SqlConnection(conexao);
            nova.Open();
            return nova;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

    }
}
