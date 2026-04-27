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
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly DbSession _session;

        public EmpresaRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Empresa empresa)
        {
            var sql = $@"
                INSERT INTO {nameof(Empresa)} (
                    {nameof(Empresa.Nome)}, 
                    {nameof(Empresa.Email)}, 
                    {nameof(Empresa.Cnpj)}, 
                    {nameof(Empresa.Telefone)}, 
                    {nameof(Empresa.DataCriacao)}
                ) 
                VALUES (@Nome, @Email,@Cnpj, @Telefone, @DataCriacao)";

            var parameters = new
            {
                empresa.Nome,
                empresa.Email,
                empresa.Cnpj,
                empresa.Telefone,
                DataCriacao = DateTime.Now
            };

            await _session._connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
        }

        public async Task Alterar(Empresa empresa)
        {
            var sql = $@"
                UPDATE {nameof(Empresa)} 
                SET 
                    {nameof(Empresa.Nome)} = @Nome, 
                    {nameof(Empresa.Email)} = @Email, 
                    {nameof(Empresa.Telefone)} = @Telefone
                WHERE {nameof(Empresa.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, empresa, transaction: _session.Transaction);
        }

        public async Task Excluir(string id)
        {
            var sql = $"DELETE FROM Empresa WHERE {nameof(Empresa.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Empresa?> ObterPorId(string id)
        {
            var sql = $"SELECT * FROM Empresa WHERE {nameof(Empresa.Id)} = @Id";
            return await _session._connection.QueryFirstOrDefaultAsync<Empresa>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<List<Empresa>> Obter()
        {
            var sql = $"SELECT * FROM Empresa ORDER BY {nameof(Empresa.Nome)}";
            var retorno =  await _session._connection.QueryAsync<Empresa>(sql, transaction: _session.Transaction);

            return retorno.ToList();
        }
    }
}
