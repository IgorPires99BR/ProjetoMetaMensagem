using FluentValidation;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.IniciaConexaoMercadoPago
{
    public class IniciaConexaoMercadoPagoHandler : IRequestHandler<IniciaConexaoMercadoPagoCommand, Response<IniciaConexaoMercadoPagoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMercadoPagoOAuth _oauth;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<IniciaConexaoMercadoPagoHandler> _logger;

        public IniciaConexaoMercadoPagoHandler(IUnitOfWork unitOfWork, IMercadoPagoOAuth oauth, ICriptografiaService criptografia,
                                               ILogger<IniciaConexaoMercadoPagoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _oauth = oauth;
            _criptografia = criptografia;
            _logger = logger;
        }

        public async Task<Response<IniciaConexaoMercadoPagoResult>> Handle(IniciaConexaoMercadoPagoCommand command)
        {
            var response = new Response<IniciaConexaoMercadoPagoResult>();
            try
            {
                var validateResult = new IniciaConexaoMercadoPagoValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                if (!_oauth.Configurado || !_criptografia.Configurada)
                {
                    response.AddErro("A conexão com o Mercado Pago não está configurada no servidor. Fale com o suporte da Contact Solution.");
                    return response;
                }

                if (await _unitOfWork.Empresa.ObterPorId(command.EmpresaId) == null)
                {
                    response.AddErro("Empresa não encontrada.", 404);
                    return response;
                }

                response.AddValue(new IniciaConexaoMercadoPagoResult
                {
                    UrlAutorizacao = _oauth.UrlAutorizacao(EstadoOAuthMercadoPago.Gerar(command.EmpresaId, HorarioBrasilia.Agora(), _criptografia))
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(IniciaConexaoMercadoPagoHandler));
            }

            return response;
        }
    }
}
