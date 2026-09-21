using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class ParametroRepository : IParametroRepository
    {
        private readonly DbSession _session;

        public ParametroRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Parametro parametro)
        {
            var sql = @"
                INSERT INTO Parametro (Id, EmpresaId, Nome, Descricao, Tipo, Valor, DataCriacao)
                VALUES (@Id, @EmpresaId, @Nome, @Descricao, @Tipo, @Valor, @DataCriacao)";

            await _session.Connection.ExecuteAsync(sql,
                new
                {
                    parametro.Id,
                    parametro.EmpresaId,
                    parametro.Nome,
                    parametro.Descricao,
                    parametro.Tipo,
                    parametro.Valor,
                    DataCriacao = DateTime.Now
                },
                transaction: _session.Transaction);
        }

        // Recorte de empresa no proprio WHERE (mesmo padrao de Perfil/Usuario): null so para a
        // conta de plataforma, que enxerga todas as empresas.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Alterar(Parametro parametro, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE Parametro
                SET Nome = @Nome, Descricao = @Descricao, Tipo = @Tipo, Valor = @Valor
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    parametro.Id,
                    parametro.Nome,
                    parametro.Descricao,
                    parametro.Tipo,
                    parametro.Valor,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM Parametro
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Parametro>> ObterPorEmpresa(Guid empresaId)
        {
            var sql = "SELECT * FROM Parametro WHERE EmpresaId = @EmpresaId ORDER BY Nome";
            return await _session.Connection.QueryAsync<Parametro>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task<int> ContarAgendamentosUsando(Guid parametroId)
        {
            // O id fica serializado dentro de VariaveisJson (ver AgendamentoVariavelDto);
            // LIKE e suficiente porque um Guid nao aparece por acaso em outro campo do JSON.
            var sql = "SELECT COUNT(1) FROM Agendamento WHERE VariaveisJson LIKE @Padrao";
            return await _session.Connection.ExecuteScalarAsync<int>(sql,
                new { Padrao = $"%{parametroId}%" }, transaction: _session.Transaction);
        }
    }
}
