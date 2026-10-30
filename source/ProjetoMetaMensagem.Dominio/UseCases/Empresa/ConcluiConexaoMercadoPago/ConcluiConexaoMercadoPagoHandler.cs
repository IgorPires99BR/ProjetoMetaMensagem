using FluentValidation;
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

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConcluiConexaoMercadoPago
{
    public class ConcluiConexaoMercadoPagoHandler : IRequestHandler<ConcluiConexaoMercadoPagoCommand, Response<ConcluiConexaoMercadoPagoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMercadoPagoOAuth _oauth;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<ConcluiConexaoMercadoPagoHandler> _logger;

        public ConcluiConexaoMercadoPagoHandler(IUnitOfWork unitOfWork, IMercadoPagoOAuth oauth, ICriptografiaService criptografia,
                                                ILogger<ConcluiConexaoMercadoPagoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _oauth = oauth;
            _criptografia = criptografia;
            _logger = logger;
        }

        public async Task<Response<ConcluiConexaoMercadoPagoResult>> Handle(ConcluiConexaoMercadoPagoCommand command)
        {
            var response = new Response<ConcluiConexaoMercadoPagoResult>();
            try
            {
                var validateResult = new ConcluiConexaoMercadoPagoValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var empresaId = EstadoOAuthMercadoPago.Ler(command.State, HorarioBrasilia.Agora(), _criptografia);
                if (empresaId == null)
                {
                    response.AddErro("O link de conexão com o Mercado Pago venceu ou é inválido. Clique em \"Conectar conta Mercado Pago\" de novo.");
                    return response;
                }

                TokensMercadoPago tokens;
                try
                {
                    tokens = await _oauth.TrocarCodigoAsync(command.Code!);
                }
                catch (PixException ex)
                {
                    response.AddErro(ex.Message);
                    return response;
                }

                // A conta da propria plataforma nao pode ser "vendedora": o marketplace_fee sairia
                // dela para ela mesma e a cobranca nao seria da empresa.
                if (_oauth.UserIdDaPlataforma != null && tokens.UserId == _oauth.UserIdDaPlataforma)
                {
                    response.AddErro("Essa é a conta Mercado Pago da própria Contact Solution. Entre com a conta Mercado Pago da empresa para conectar.");
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(empresaId.Value)
                            ?? new DadosBancariosEmpresa { EmpresaId = empresaId.Value };
                dados.ProvedorRecebimento = ProvedorCobranca.MercadoPago;
                dados.MercadoPagoUserId = tokens.UserId;
                dados.MercadoPagoAccessTokenCriptografado = _criptografia.Criptografar(tokens.AccessToken);
                dados.MercadoPagoRefreshTokenCriptografado = _criptografia.Criptografar(tokens.RefreshToken);
                dados.MercadoPagoTokenExpiraEm = tokens.ExpiraEm;
                dados.MercadoPagoConectadoEm = HorarioBrasilia.Agora();

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

                _logger.LogInformation("MercadoPago: empresa {EmpresaId} conectou a conta {UserId}.", empresaId, tokens.UserId);
                response.AddValue(new ConcluiConexaoMercadoPagoResult { EmpresaId = empresaId.Value, MercadoPagoUserId = tokens.UserId });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ConcluiConexaoMercadoPagoHandler));
            }

            return response;
        }
    }
}
