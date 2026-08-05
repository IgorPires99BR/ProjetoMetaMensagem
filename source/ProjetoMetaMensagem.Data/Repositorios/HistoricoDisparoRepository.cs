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
    public class HistoricoDisparoRepository : IHistoricoDisparoRepository
    {
        private readonly DbSession _session;

        public HistoricoDisparoRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(HistoricoDisparo historico)
        {
            var sql = $@"
                INSERT INTO {nameof(HistoricoDisparo)} (
                    {nameof(historico.Id)}, 
                    {nameof(historico.EmpresaId)}, 
                    {nameof(historico.ContatoId)}, 
                    {nameof(historico.TemplateId)}, 
                    {nameof(historico.TipoDisparo)}, 
                    {nameof(historico.Conteudo)}, 
                    {nameof(historico.WamidMeta)}, 
                    {nameof(historico.DataEnvio)}
                ) 
                VALUES (
                    @{nameof(historico.Id)}, 
                    @{nameof(historico.EmpresaId)}, 
                    @{nameof(historico.ContatoId)}, 
                    @{nameof(historico.TemplateId)}, 
                    @{nameof(historico.TipoDisparo)}, 
                    @{nameof(historico.Conteudo)}, 
                    @{nameof(historico.WamidMeta)}, 
                    @{nameof(historico.DataEnvio)}
                );";

            // Garante que a data seja preenchida caso não tenha sido setada na entidade
            if (historico.DataEnvio == default)
            {
                historico.DataEnvio = DateTime.Now;
            }

            await _session.Connection.ExecuteAsync(sql, historico, transaction: _session.Transaction);
        }

        public async Task<HistoricoDisparo?> ObterPorId(Guid id)
        {
            var sql = $@"
                SELECT * FROM {nameof(HistoricoDisparo)} 
                WHERE {nameof(HistoricoDisparo.Id)} = @{nameof(HistoricoDisparo.Id)}";

            return await _session.Connection.QueryFirstOrDefaultAsync<HistoricoDisparo>(
                sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<HistoricoDisparo>> ListarPorContato(Guid empresaId, Guid contatoId)
        {
            var sql = $@"
        SELECT * FROM {nameof(HistoricoDisparo)} 
        WHERE {nameof(HistoricoDisparo.EmpresaId)} = @EmpresaId 
          AND {nameof(HistoricoDisparo.ContatoId)} = @ContatoId
        ORDER BY {nameof(HistoricoDisparo.DataEnvio)} ASC";

            return await _session.Connection.QueryAsync<HistoricoDisparo>(
                sql,
                new { EmpresaId = empresaId, ContatoId = contatoId },
                transaction: _session.Transaction
            );
        }

        public async Task<IEnumerable<HistoricoDisparoComTelefone>> ListarPorEmpresa(Guid empresaId)
        {
            var sql = $@"
        SELECT h.*, c.Telefone AS TelefoneContato
        FROM {nameof(HistoricoDisparo)} h
        LEFT JOIN Contato c ON c.Id = h.{nameof(HistoricoDisparo.ContatoId)}
        WHERE h.{nameof(HistoricoDisparo.EmpresaId)} = @EmpresaId
        ORDER BY h.{nameof(HistoricoDisparo.DataEnvio)} ASC";

            return await _session.Connection.QueryAsync<HistoricoDisparoComTelefone>(
                sql,
                new { EmpresaId = empresaId },
                transaction: _session.Transaction
            );
        }

        public async Task<HistoricoDisparo?> ObterPorWamidMeta(string wamidMeta)
        {
            var sql = $@"
                SELECT * FROM {nameof(HistoricoDisparo)} 
                WHERE {nameof(HistoricoDisparo.WamidMeta)} = @{nameof(HistoricoDisparo.WamidMeta)}";

            return await _session.Connection.QueryFirstOrDefaultAsync<HistoricoDisparo>(
                sql, new { WamidMeta = wamidMeta }, transaction: _session.Transaction);
        }

    }
}

