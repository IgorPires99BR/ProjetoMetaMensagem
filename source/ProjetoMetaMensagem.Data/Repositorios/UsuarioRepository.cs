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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbSession _session;

        public UsuarioRepository(DbSession session)
        {
            _session = session;
        }

        public async Task Incluir(Usuario usuario)
        {
            // Id e IsAdmin ficavam de fora do INSERT: o banco preenchia Id com NEWID() (jogando
            // fora o id que a aplicacao gerou e devolveu na resposta) e IsAdmin com 0 -- ou seja,
            // nao havia como criar um usuario administrador pela API. Mesmo bug que o Contato ja
            // teve. Quando o chamador nao define o Id, ele vem de Guid.NewGuid() na entidade.
            var sql = $@"
                INSERT INTO {nameof(Usuario)} (
                    {nameof(Usuario.Id)},
                    {nameof(Usuario.EmpresaId)},
                    {nameof(Usuario.Nome)},
                    {nameof(Usuario.Email)},
                    {nameof(Usuario.SenhaHash)},
                    {nameof(Usuario.IsAdmin)},
                    {nameof(Usuario.DataCriacao)}
                )
                VALUES (
                    @Id, @EmpresaId, @Nome, @Email, @SenhaHash, @IsAdmin, @DataCriacao
                )";

            var parameters = new DynamicParameters();
            parameters.Add("Id", usuario.Id == Guid.Empty ? Guid.NewGuid() : usuario.Id);
            parameters.Add("EmpresaId", usuario.EmpresaId);
            parameters.Add("Nome", usuario.Nome);
            parameters.Add("Email", usuario.Email);
            parameters.Add("SenhaHash", usuario.SenhaHash);
            // A coluna e NOT NULL com default 0; a entidade usa bool? porque nem todo caminho
            // de criacao informa o campo.
            parameters.Add("IsAdmin", usuario.IsAdmin ?? false);
            parameters.Add("DataCriacao", DateTimeOffset.Now);

            await _session.Connection.ExecuteAsync(sql, parameters, transaction: _session.Transaction);
        }

        // Recorte de empresa aplicado direto no WHERE. Antes o UPDATE/DELETE casava so pelo Id,
        // entao bastava conhecer (ou adivinhar) o id pra alterar/apagar usuario de outra empresa.
        private const string RecorteDaEmpresa = @"
              AND (@EmpresaIdSolicitante IS NULL OR EmpresaId = @EmpresaIdSolicitante)";

        // empresaIdSolicitante e opcional porque a redefinicao de senha (EsqueceuASenha) roda
        // sem usuario logado -- ali nao existe escopo a aplicar e o recorte fica desligado.
        public async Task<int> Alterar(Usuario usuario, Guid? empresaIdSolicitante = null)
        {
            var sql = $@"
                UPDATE {nameof(Usuario)}
                SET
                    {nameof(Usuario.Nome)} = @Nome,
                    {nameof(Usuario.Email)} = @Email,
                    {nameof(Usuario.SenhaHash)} = @SenhaHash
                WHERE {nameof(Usuario.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    usuario.SenhaHash,
                    EmpresaIdSolicitante = empresaIdSolicitante
                },
                transaction: _session.Transaction);
        }

        public async Task<Usuario?> ObterPorEmail(string email)
        {
            var sql = $@"
                SELECT * FROM {nameof(Usuario)}
                WHERE {nameof(Usuario.Email)} = @Email";

            return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new { Email = email },
                transaction: _session.Transaction
            );
        }

        public async Task<Usuario?> Logar(string email, string senhaHash)
        {
            var sql = $@"
             SELECT * FROM {nameof(Usuario)}
             WHERE {nameof(Usuario.Email)} = @Email
               AND {nameof(Usuario.SenhaHash)} = @SenhaHash";

            return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new { Email = email, SenhaHash = senhaHash },
                transaction: _session.Transaction
            );
        }

        public async Task<int> Excluir(string id, Guid? empresaIdSolicitante)
        {
            var sql = $@"
                DELETE FROM {nameof(Usuario)}
                WHERE {nameof(Usuario.Id)} = @Id
                {RecorteDaEmpresa}";

            return await _session.Connection.ExecuteAsync(sql,
                new { Id = id, EmpresaIdSolicitante = empresaIdSolicitante },
                transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Usuario>> ObterPorEmpresa(Guid id)
        {
            var sql = $"SELECT * FROM {nameof(Usuario)} WHERE {nameof(Usuario.EmpresaId)} = @Id";
            return await _session.Connection.QueryAsync<Usuario>(sql, new { Id = id }, transaction: _session.Transaction);
        }

        public async Task<IEnumerable<Usuario>> Obter()
        {
            var sql = $"SELECT * FROM {nameof(Usuario)} ORDER BY {nameof(Usuario.Nome)}";
            return await _session.Connection.QueryAsync<Usuario>(sql, transaction: _session.Transaction);
        }

        public async Task<Usuario?> ObterPorId(Guid id)
        {
            var sql = $"SELECT * FROM {nameof(Usuario)} WHERE {nameof(Usuario.Id)} = @Id";
            return await _session.Connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id }, transaction: _session.Transaction);
        }
    }
}

