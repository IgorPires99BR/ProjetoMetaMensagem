using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class TemplateConexaoRepository : ITemplateConexaoRepository
    {
        private readonly DbSession _session;

        public TemplateConexaoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(TemplateConexao templateConexao)
        {
            var sql = $@"
                INSERT INTO {nameof(TemplateConexao)} (
                    {nameof(TemplateConexao.Id)},
                    {nameof(TemplateConexao.EmpresaId)},
                    {nameof(TemplateConexao.TemplateOrigemId)},
                    {nameof(TemplateConexao.TemplateDestinoId)},
                    {nameof(TemplateConexao.BotaoTexto)},
                    {nameof(TemplateConexao.DataCriacao)}
                )
                VALUES (@Id, @EmpresaId, @TemplateOrigemId, @TemplateDestinoId, @BotaoTexto, @DataCriacao);";

            await _session.Connection.ExecuteAsync(sql, templateConexao, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<TemplateConexao>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(TemplateConexao)} WHERE {nameof(TemplateConexao.EmpresaId)} = @EmpresaId";
            return await _session.Connection.QueryAsync<TemplateConexao>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            // Recorte de empresa no WHERE: antes o DELETE casava so pelo Id e qualquer usuario
            // logado apagava a conexao de outra empresa mandando o id na rota.
            var sql = $@"
                DELETE FROM {nameof(TemplateConexao)}
                WHERE {nameof(TemplateConexao.Id)} = @Id
                  AND (@EmpresaIdSolicitante IS NULL
                       OR {nameof(TemplateConexao.EmpresaId)} = @EmpresaIdSolicitante)";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }
    }
}
