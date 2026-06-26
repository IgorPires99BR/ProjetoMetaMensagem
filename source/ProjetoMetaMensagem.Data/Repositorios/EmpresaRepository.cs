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

        public async Task<Guid> Incluir(Empresa empresa)
        {
            // Garante que a empresa tenha um ID antes de ir para o banco, caso não venha preenchido
            if (empresa.Id == Guid.Empty)
            {
                empresa.Id = Guid.NewGuid();
            }

            var sql = $@"
                INSERT INTO {nameof(Empresa)} (
                    {nameof(Empresa.Id)},
                    {nameof(Empresa.Nome)}, 
                    {nameof(Empresa.Email)}, 
                    {nameof(Empresa.Cnpj)}, 
                    {nameof(Empresa.MetaAccessToken)}, 
                    {nameof(Empresa.PhoneNumberId)}, 
                    {nameof(Empresa.WabaId)}, 
                    {nameof(Empresa.PlanoId)}, 
                    {nameof(Empresa.Telefone)}, 
                    {nameof(Empresa.DataCriacao)}
                ) 
                VALUES (@Id, @Nome, @Email, @Cnpj, @MetaAccessToken,@PhoneNumberId,@WabaId, @PlanoId, @Telefone, @DataCriacao)";

            var parameters = new
            {
                empresa.Id,
                empresa.Nome,
                empresa.Email,
                empresa.Cnpj,
                empresa.MetaAccessToken,
                empresa.PhoneNumberId,
                empresa.WabaId,
                empresa.PlanoId,
                empresa.Telefone,
                DataCriacao = DateTime.Now
            };

            await _session._connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);

            return empresa.Id;
        }

        public async Task<Guid> Alterar(Empresa empresa)
        {
            var sql = $@"
                UPDATE {nameof(Empresa)} 
                SET 
                    {nameof(Empresa.Nome)} = @Nome, 
                    {nameof(Empresa.Email)} = @Email, 
                    {nameof(Empresa.Cnpj)} = @Cnpj, 
                    {nameof(Empresa.Telefone)} = @Telefone,
                    {nameof(Empresa.WabaId)} = @WabaId,
                    {nameof(Empresa.PhoneNumberId)} = @PhoneNumberId,
                    {nameof(Empresa.MetaAccessToken)} = @MetaAccessToken,
                    {nameof(Empresa.PlanoId)} = @PlanoId
                WHERE {nameof(Empresa.Id)} = @Id";

            await _session._connection.ExecuteAsync(sql, empresa, transaction: _session.Transaction);

            return empresa.Id;
        }

        public async Task Excluir(string id)
        {
            var sql = $"DELETE FROM Empresa WHERE {nameof(Empresa.Id)} = @Id";
            await _session._connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<string?> ObterMetaAccessToken(Guid id)
        {
            var sql = $@"SELECT {nameof(Empresa.MetaAccessToken)} 
                 FROM {nameof(Empresa)} 
                 WHERE {nameof(Empresa.Id)} = @Id";

            return await _session._connection.QueryFirstOrDefaultAsync<string>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task AtualizarWabaId(Guid id, string wabaId)
        {
            var sql = $@"
        UPDATE {nameof(Empresa)} 
        SET {nameof(Empresa.WabaId)} = @WabaId 
        WHERE {nameof(Empresa.Id)} = @Id";

            var parameters = new
            {
                Id = id,
                WabaId = wabaId
            };

            await _session._connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
        }

        public async Task<string?> ObterWabaId(Guid id)
        {
            var sql = $@"SELECT {nameof(Empresa.WabaId)} 
                 FROM {nameof(Empresa)} 
                 WHERE {nameof(Empresa.Id)} = @Id";

            return await _session._connection.QueryFirstOrDefaultAsync<string>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<string?> ObterPhoneNumberId(Guid id)
        {
            var sql = $@"SELECT {nameof(Empresa.PhoneNumberId)} 
                 FROM {nameof(Empresa)} 
                 WHERE {nameof(Empresa.Id)} = @Id";

            return await _session._connection.QueryFirstOrDefaultAsync<string>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Empresa?> ObterPorId(Guid id)
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
