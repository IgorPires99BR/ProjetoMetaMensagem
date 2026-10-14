using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
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

                if (await _unitOfWork.Empresa.ObterPorId(command.EmpresaId) == null)
                {
                    response.AddErro("Empresa não encontrada.", 404);
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId)
                            ?? new DadosBancariosEmpresa { EmpresaId = command.EmpresaId };

                // Mesma regra da criacao: trocar a conta que recebe o dinheiro e decisao do suporte.
                if (dados.TemSubconta)
                {
                    response.AddErro("Esta empresa já tem uma subconta Asaas. Para trocar, fale com o suporte da Contact Solution.");
                    return response;
                }

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

                // Repasse para uma carteira fora da nossa conta nao funcionaria (e poderia mandar
                // dinheiro para a pessoa errada): so aceita subconta da plataforma.
                if (subconta == null)
                {
                    response.AddErro("Nenhuma subconta da conta Asaas da plataforma tem esse walletId. Confira o valor no painel do Asaas.");
                    return response;
                }

                dados.AsaasContaId = subconta.Id;
                dados.AsaasWalletId = subconta.WalletId;
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

                _logger.LogInformation("AsaasPix: subconta {ContaId} (wallet {WalletId}) vinculada a empresa {EmpresaId}",
                    subconta.Id, subconta.WalletId, command.EmpresaId);

                dados.DataAtualizacao = DateTime.Now;
                response.AddValue(new VinculaSubcontaAsaasResult
                {
                    DadosBancarios = new ObtemDadosBancariosResult(command.EmpresaId, dados),
                    NomeSubconta = subconta.Nome
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
