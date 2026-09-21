using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class PerfilRepository : IPerfilRepository
    {
        private readonly DbSession _session;

        public PerfilRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Perfil perfil)
        {
            var sqlPerfil = @"
                INSERT INTO Perfil (Id, EmpresaId, Nome, DataCriacao)
                VALUES (@Id, @EmpresaId, @Nome, @DataCriacao);";

            await _session.Connection.ExecuteAsync(sqlPerfil,
                new { perfil.Id, perfil.EmpresaId, perfil.Nome, DataCriacao = DateTime.Now },
                transaction: _session.Transaction);

            await GravarTelas(perfil.Id, perfil.Telas);
        }

        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        public async Task<int> Alterar(Perfil perfil, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                UPDATE Perfil
                SET Nome = @Nome
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            var linhas = await _session.Connection.ExecuteAsync(sql,
                new { perfil.Id, perfil.Nome, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);

            // Zero linhas = perfil inexistente ou de outra empresa: nao mexe nas telas.
            if (linhas > 0)
            {
                await _session.Connection.ExecuteAsync("DELETE FROM PerfilTela WHERE PerfilId = @Id",
                    new { perfil.Id }, transaction: _session.Transaction);
                await GravarTelas(perfil.Id, perfil.Telas);
            }

            return linhas;
        }

        private async Task GravarTelas(Guid perfilId, IEnumerable<string> telas)
        {
            var telasLista = telas?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList() ?? new List<string>();
            if (telasLista.Count == 0) return;

            var sql = "INSERT INTO PerfilTela (PerfilId, TelaChave) VALUES (@PerfilId, @TelaChave)";
            var parametros = telasLista.Select(t => new { PerfilId = perfilId, TelaChave = t });
            await _session.Connection.ExecuteAsync(sql, parametros, transaction: _session.Transaction);
        }

        public async Task<int> Excluir(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM Perfil
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<Perfil?> ObterPorId(Guid id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                SELECT * FROM Perfil
                WHERE Id = @Id
                {RecorteDaEmpresa}";

            var perfil = await _session.Connection.QueryFirstOrDefaultAsync<Perfil>(
                sql, new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante }, transaction: _session.Transaction);

            if (perfil != null)
            {
                perfil.Telas = await ObterTelasDoPerfil(perfil.Id) ?? new List<string>();
            }

            return perfil;
        }

        public async Task<IEnumerable<Perfil>> ObterPorEmpresa(Guid? empresaId)
        {
            var sql = @"
                SELECT * FROM Perfil
                WHERE (@EmpresaId IS NULL OR EmpresaId = @EmpresaId)
                ORDER BY Nome";

            var perfis = (await _session.Connection.QueryAsync<Perfil>(
                sql, new { EmpresaId = empresaId }, transaction: _session.Transaction)).ToList();

            if (perfis.Count == 0) return perfis;

            var telasSql = "SELECT PerfilId, TelaChave FROM PerfilTela WHERE PerfilId IN @Ids";
            var telas = await _session.Connection.QueryAsync<(Guid PerfilId, string TelaChave)>(
                telasSql, new { Ids = perfis.Select(p => p.Id).ToList() }, transaction: _session.Transaction);

            var telasPorPerfil = telas.GroupBy(t => t.PerfilId).ToDictionary(g => g.Key, g => g.Select(t => t.TelaChave).ToList());

            foreach (var perfil in perfis)
            {
                perfil.Telas = telasPorPerfil.TryGetValue(perfil.Id, out var t) ? t : new List<string>();
            }

            return perfis;
        }

        public async Task<List<string>?> ObterTelasDoPerfil(Guid? perfilId)
        {
            if (!perfilId.HasValue) return null;

            var sql = "SELECT TelaChave FROM PerfilTela WHERE PerfilId = @PerfilId";
            var telas = await _session.Connection.QueryAsync<string>(
                sql, new { PerfilId = perfilId.Value }, transaction: _session.Transaction);

            return telas.ToList();
        }
    }
}
