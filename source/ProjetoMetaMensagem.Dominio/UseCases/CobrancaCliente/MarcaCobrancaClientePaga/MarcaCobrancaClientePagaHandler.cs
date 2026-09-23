using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga
{
    public class MarcaCobrancaClientePagaHandler : IRequestHandler<MarcaCobrancaClientePagaCommand, Response<MarcaCobrancaClientePagaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarcaCobrancaClientePagaHandler> _logger;

        public MarcaCobrancaClientePagaHandler(IUnitOfWork unitOfWork, ILogger<MarcaCobrancaClientePagaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<MarcaCobrancaClientePagaResult>> Handle(MarcaCobrancaClientePagaCommand command)
        {
            var response = new Response<MarcaCobrancaClientePagaResult>();

            var validator = new MarcaCobrancaClientePagaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorId(command.CobrancaClienteId, command.EmpresaIdSolicitante);

                if (cobranca == null)
                {
                    response.AddErro("Cobrança não encontrada.");
                    return response;
                }

                if (cobranca.Status == StatusCobrancaCliente.Paga)
                {
                    response.AddErro("Esta cobrança já está marcada como paga.");
                    return response;
                }

                if (cobranca.Status == StatusCobrancaCliente.Cancelada)
                {
                    response.AddErro("Esta cobrança foi cancelada e não pode ser marcada como paga.");
                    return response;
                }

                var dataPagamento = command.DataPagamento ?? DateTime.Now;

                await _unitOfWork.CobrancaCliente.MarcarPaga(command.CobrancaClienteId, command.EmpresaIdSolicitante, dataPagamento);

                response.AddValue(new MarcaCobrancaClientePagaResult
                {
                    CobrancaClienteId = command.CobrancaClienteId,
                    DataPagamento = dataPagamento
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(MarcaCobrancaClientePagaHandler));
            }

            return response;
        }
    }
}
