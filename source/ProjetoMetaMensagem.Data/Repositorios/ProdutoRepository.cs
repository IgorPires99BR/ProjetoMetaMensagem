using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly DbSession _session;

        public ProdutoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<Guid> Incluir(Produto produto)
        {
            var sql = $@"
                INSERT INTO {nameof(Produto)} (
                    {nameof(Produto.Id)},
                    {nameof(Produto.EmpresaId)},
                    {nameof(Produto.Nome)},
                    {nameof(Produto.Descricao)},
                    {nameof(Produto.Preco)},
                    {nameof(Produto.ImagemUrl)},
                    {nameof(Produto.LinkUrl)},
                    {nameof(Produto.Categoria)},
                    {nameof(Produto.Ativo)},
                    {nameof(Produto.DataCriacao)}
                ) VALUES (
                    @{nameof(Produto.Id)},
                    @{nameof(Produto.EmpresaId)},
                    @{nameof(Produto.Nome)},
                    @{nameof(Produto.Descricao)},
                    @{nameof(Produto.Preco)},
                    @{nameof(Produto.ImagemUrl)},
                    @{nameof(Produto.LinkUrl)},
                    @{nameof(Produto.Categoria)},
                    @{nameof(Produto.Ativo)},
                    @{nameof(Produto.DataCriacao)}
                );";

            await _session.Connection.ExecuteAsync(sql, produto, transaction: _session.Transaction);
            return produto.Id;
        }

        public async Task Alterar(Produto produto)
        {
            var sql = $@"
                UPDATE {nameof(Produto)}
                SET
                    {nameof(Produto.Nome)} = @{nameof(Produto.Nome)},
                    {nameof(Produto.Descricao)} = @{nameof(Produto.Descricao)},
                    {nameof(Produto.Preco)} = @{nameof(Produto.Preco)},
                    {nameof(Produto.ImagemUrl)} = @{nameof(Produto.ImagemUrl)},
                    {nameof(Produto.LinkUrl)} = @{nameof(Produto.LinkUrl)},
                    {nameof(Produto.Categoria)} = @{nameof(Produto.Categoria)},
                    {nameof(Produto.Ativo)} = @{nameof(Produto.Ativo)}
                WHERE {nameof(Produto.Id)} = @{nameof(Produto.Id)}";

            await _session.Connection.ExecuteAsync(sql, produto, transaction: _session.Transaction);
        }

        public async Task Excluir(Guid id)
        {
            var sql = $@"
                DELETE FROM {nameof(Produto)}
                WHERE {nameof(Produto.Id)} = @Id";

            await _session.Connection.ExecuteAsync(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<Produto?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(Produto)}
                WHERE {nameof(Produto.Id)} = @Id";

            return await _session.Connection.QueryFirstOrDefaultAsync<Produto>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Produto>> Obter()
        {
            var sql = $@"
                SELECT * FROM {nameof(Produto)}
                ORDER BY {nameof(Produto.DataCriacao)} DESC";

            return await _session.Connection.QueryAsync<Produto>(sql, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Produto>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = $@"
                SELECT * FROM {nameof(Produto)}
                WHERE {nameof(Produto.EmpresaId)} = @EmpresaId
                ORDER BY {nameof(Produto.Nome)}";

            return await _session.Connection.QueryAsync<Produto>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }
    }
}

