using FluentValidation;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DesconectaMercadoPago
{
    public class DesconectaMercadoPagoHandler : IRequestHandler<DesconectaMercadoPagoCommand, Response<ObtemDadosBancariosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DesconectaMercadoPagoHandler> _logger;

        public DesconectaMercadoPagoHandler(IUnitOfWork unitOfWork, ILogger<DesconectaMercadoPagoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemDadosBancariosResult>> Handle(DesconectaMercadoPagoCommand command)
        {
            var response = new Response<ObtemDadosBancariosResult>();
            try
            {
                var validateResult = new DesconectaMercadoPagoValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId);
                if (dados == null || !dados.MercadoPagoConectado)
                {
                    response.AddErro("A empresa não tem conta Mercado Pago conectada.");
                    return response;
                }

                var userId = dados.MercadoPagoUserId;
                dados.DesconectarMercadoPago();

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

                _logger.LogInformation("MercadoPago: empresa {EmpresaId} desconectou a conta {UserId}.", command.EmpresaId, userId);
                dados.DataAtualizacao = DateTime.Now;
                response.AddValue(new ObtemDadosBancariosResult(command.EmpresaId, dados));
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(DesconectaMercadoPagoHandler));
            }

            return response;
        }
    }
}
