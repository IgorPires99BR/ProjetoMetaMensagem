using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.SimulaPagamentoSandbox
{
    public class SimulaPagamentoSandboxHandler : IRequestHandler<SimulaPagamentoSandboxCommand, Response<SimulaPagamentoSandboxResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPixService _pix;
        private readonly IBaixaCobrancaPix _baixa;
        private readonly ILogger<SimulaPagamentoSandboxHandler> _logger;

        public SimulaPagamentoSandboxHandler(IUnitOfWork unitOfWork, IPixService pix, IBaixaCobrancaPix baixa,
            ILogger<SimulaPagamentoSandboxHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _baixa = baixa;
            _logger = logger;
        }

        public async Task<Response<SimulaPagamentoSandboxResult>> Handle(SimulaPagamentoSandboxCommand command)
        {
            var response = new Response<SimulaPagamentoSandboxResult>();

            var validateResult = new SimulaPagamentoSandboxValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            if (!_pix.Sandbox)
            {
                response.AddErro("Simular pagamento só funciona com o Asaas em SANDBOX.", 403);
                return response;
            }

            try
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorId(command.CobrancaClienteId, command.EmpresaIdSolicitante);
                if (cobranca == null)
                {
                    response.AddErro("Cobrança não encontrada.", 404);
                    return response;
                }

                // O sandbox do Mercado Pago nao tem rota para "pagar" uma order: o teste e com
                // credenciais e usuario comprador de teste.
                if (cobranca.Status != StatusCobrancaCliente.Pendente || string.IsNullOrEmpty(cobranca.PixIdExterno) || string.IsNullOrEmpty(cobranca.Txid)
                    || (cobranca.Provedor ?? ProvedorCobranca.Asaas) != ProvedorCobranca.Asaas)
                {
                    response.AddErro("Só dá para simular o pagamento de uma cobrança pendente com Pix do Asaas.");
                    return response;
                }

                string statusAsaas;
                try
                {
                    statusAsaas = await _pix.ConfirmarPagamentoSandboxAsync(cobranca.PixIdExterno);
                }
                catch (PixException ex)
                {
                    response.AddErro(ex.Message);
                    return response;
                }

                // Mesmo caminho do webhook: reconsulta o Asaas e so baixa se la estiver pago.
                await _baixa.ProcessarWebhookAsync(new[] { cobranca.Txid });

                var depois = await _unitOfWork.CobrancaCliente.ObterPorId(command.CobrancaClienteId, command.EmpresaIdSolicitante);

                _logger.LogInformation("AsaasPix: pagamento simulado no sandbox para a cobranca {CobrancaId} ({IdExterno}): Asaas {StatusAsaas}, cobranca {Status}.",
                    cobranca.Id, cobranca.PixIdExterno, statusAsaas, depois?.Status);

                response.AddValue(new SimulaPagamentoSandboxResult
                {
                    CobrancaClienteId = cobranca.Id,
                    StatusAsaas = statusAsaas,
                    Status = depois?.Status ?? cobranca.Status
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(SimulaPagamentoSandboxHandler));
            }

            return response;
        }
    }
}
