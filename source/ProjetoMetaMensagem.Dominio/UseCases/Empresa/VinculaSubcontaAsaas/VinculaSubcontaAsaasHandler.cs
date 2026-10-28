using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.VinculaSubcontaAsaas
{
    public class VinculaSubcontaAsaasHandler : IRequestHandler<VinculaSubcontaAsaasCommand, Response<VinculaSubcontaAsaasResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPixService _pix;
        private readonly ILogger<VinculaSubcontaAsaasHandler> _logger;

        public VinculaSubcontaAsaasHandler(IUnitOfWork unitOfWork, IPixService pix, ILogger<VinculaSubcontaAsaasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _logger = logger;
        }

        public async Task<Response<VinculaSubcontaAsaasResult>> Handle(VinculaSubcontaAsaasCommand command)
        {
            var response = new Response<VinculaSubcontaAsaasResult>();

            try
            {
                command.WalletId = command.WalletId?.Trim().ToLowerInvariant();

                var validateResult = new VinculaSubcontaAsaasValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var empresa = await _unitOfWork.Empresa.ObterPorId(command.EmpresaId);
                if (empresa == null)
                {
                    response.AddErro("Empresa não encontrada.", 404);
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId)
                            ?? new DadosBancariosEmpresa { EmpresaId = command.EmpresaId };

                // Trocar a conta que recebe o dinheiro so de proposito: o endpoint ja e restrito a
                // conta da plataforma, e a troca precisa vir marcada.
                if (dados.TemSubconta && !command.Substituir)
                {
                    response.AddErro("Esta empresa já tem uma conta Asaas vinculada para o repasse. Marque a opção de substituir para trocar.");
                    return response;
                }
                var walletAnterior = dados.AsaasWalletId;

                SubcontaExistente? subconta;
                try
                {
                    subconta = await _pix.ObterSubcontaAsync(command.WalletId!);
                }
                catch (PixException ex)
                {
                    response.AddErro($"Não foi possível consultar a subconta no Asaas: {ex.Message}");
                    return response;
                }

                // Carteira fora da plataforma so com a marcacao explicita: um walletId digitado
                // errado que por acaso exista mandaria o repasse para outra pessoa.
                if (subconta == null && !command.CarteiraExterna)
                {
                    response.AddErro("Nenhuma subconta da conta Asaas da plataforma tem esse walletId. Confira o valor no painel do Asaas ou, se for a conta Asaas própria da empresa, marque essa opção.");
                    return response;
                }

                // Sem subconta nao ha CNPJ no Asaas para o cliente padrao (contato sem CPF/CNPJ):
                // ele sai do cadastro da empresa.
                if (subconta == null && !DocumentoHelper.CpfCnpjValido(empresa.Cnpj))
                {
                    response.AddErro("Para vincular a conta Asaas própria, cadastre antes um CNPJ válido na empresa.");
                    return response;
                }

                dados.AsaasContaId = subconta?.Id;
                dados.AsaasWalletId = subconta?.WalletId ?? command.WalletId;
                // Aberta pelo painel: a apiKey da subconta nunca passou por aqui.
                dados.AsaasApiKeyCriptografada = null;
                dados.AsaasContaCriadaEm = DateTime.Now;

                _unitOfWork.BeginTransaction();
                try
                {
                    await _unitOfWork.DadosBancariosEmpresa.Salvar(dados);
                }
                catch
                {
                    _unitOfWork.Rollback();
                    throw;
                }
                _unitOfWork.Commit();

                _logger.LogInformation("AsaasPix: {Tipo} {ContaId} (wallet {WalletId}) vinculada a empresa {EmpresaId}; wallet anterior: {WalletAnterior}",
                    subconta == null ? "carteira externa" : "subconta", subconta?.Id ?? "-", dados.AsaasWalletId, command.EmpresaId, walletAnterior ?? "-");

                dados.DataAtualizacao = DateTime.Now;
                response.AddValue(new VinculaSubcontaAsaasResult
                {
                    DadosBancarios = new ObtemDadosBancariosResult(command.EmpresaId, dados),
                    NomeSubconta = subconta?.Nome ?? empresa.Nome
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(VinculaSubcontaAsaasHandler));
            }

            return response;
        }
    }
}
