using Dapper;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Data.Repositorios
{
    public class DadosBancariosEmpresaRepository : IDadosBancariosEmpresaRepository
    {
        private readonly DbSession _session;

        public DadosBancariosEmpresaRepository(DbSession session)
        {
            _session = session;
        }

        public async Task<DadosBancariosEmpresa?> ObterPorEmpresa(Guid empresaId)
        {
            var sql = $"SELECT * FROM {nameof(DadosBancariosEmpresa)} WHERE {nameof(DadosBancariosEmpresa.EmpresaId)} = @EmpresaId";
            return await _session.Connection.QueryFirstOrDefaultAsync<DadosBancariosEmpresa>(sql, new { EmpresaId = empresaId }, transaction: _session.Transaction);
        }

        public async Task Salvar(DadosBancariosEmpresa dados)
        {
            var sql = @"
                UPDATE DadosBancariosEmpresa
                SET Banco = @Banco,
                    Agencia = @Agencia,
                    Conta = @Conta,
                    ContaDigito = @ContaDigito,
                    TitularNome = @TitularNome,
                    TitularDocumento = @TitularDocumento,
                    TipoChavePix = @TipoChavePix,
                    ChavePix = @ChavePix,
                    Ambiente = @Ambiente,
                    ClientId = @ClientId,
                    ClientSecretCriptografado = @ClientSecretCriptografado,
                    CertificadoPem = @CertificadoPem,
                    ChavePrivadaCriptografada = @ChavePrivadaCriptografada,
                    CertificadoValidoAte = @CertificadoValidoAte,
                    CertificadoTitular = @CertificadoTitular,
                    CobrancaPixAtiva = @CobrancaPixAtiva,
                    DataAtualizacao = GETDATE()
                WHERE EmpresaId = @EmpresaId;

                IF @@ROWCOUNT = 0
                    INSERT INTO DadosBancariosEmpresa (
                        EmpresaId, Banco, Agencia, Conta, ContaDigito, TitularNome, TitularDocumento,
                        TipoChavePix, ChavePix, Ambiente, ClientId, ClientSecretCriptografado,
                        CertificadoPem, ChavePrivadaCriptografada, CertificadoValidoAte, CertificadoTitular,
                        CobrancaPixAtiva, DataCriacao)
                    VALUES (
                        @EmpresaId, @Banco, @Agencia, @Conta, @ContaDigito, @TitularNome, @TitularDocumento,
                        @TipoChavePix, @ChavePix, @Ambiente, @ClientId, @ClientSecretCriptografado,
                        @CertificadoPem, @ChavePrivadaCriptografada, @CertificadoValidoAte, @CertificadoTitular,
                        @CobrancaPixAtiva, GETDATE());";

            await _session.Connection.ExecuteAsync(sql, dados, transaction: _session.Transaction);
        }
    }
}
