using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Job diario: renova o token da conta Mercado Pago das empresas antes dos 180 dias. Vencido, a
    // cobranca volta para a conta da Contact Solution (DestinoRecebimento) ate a empresa reconectar.
    public interface IRenovacaoContasMercadoPago
    {
        Task RenovarVencendoAsync();
    }

    public class RenovacaoContasMercadoPago : IRenovacaoContasMercadoPago
    {
        // Folga larga: uma semana de job falhando (Mercado Pago fora, deploy) ainda nao vence ninguem.
        private static readonly TimeSpan Antecedencia = TimeSpan.FromDays(30);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMercadoPagoOAuth _oauth;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<RenovacaoContasMercadoPago> _logger;

        public RenovacaoContasMercadoPago(IUnitOfWork unitOfWork, IMercadoPagoOAuth oauth, ICriptografiaService criptografia,
                                          ILogger<RenovacaoContasMercadoPago> logger)
        {
            _unitOfWork = unitOfWork;
            _oauth = oauth;
            _criptografia = criptografia;
            _logger = logger;
        }

        public async Task RenovarVencendoAsync()
        {
            if (!_oauth.Configurado || !_criptografia.Configurada) return;

            var vencendo = (await _unitOfWork.DadosBancariosEmpresa.ObterComMercadoPagoVencendo(HorarioBrasilia.Agora() + Antecedencia)).ToList();
            foreach (var dados in vencendo)
            {
                try
                {
                    var tokens = await _oauth.RenovarAsync(_criptografia.Descriptografar(dados.MercadoPagoRefreshTokenCriptografado!));
                    dados.MercadoPagoAccessTokenCriptografado = _criptografia.Criptografar(tokens.AccessToken);
                    dados.MercadoPagoRefreshTokenCriptografado = _criptografia.Criptografar(tokens.RefreshToken);
                    dados.MercadoPagoTokenExpiraEm = tokens.ExpiraEm;
                    dados.MercadoPagoUserId = tokens.UserId;

                    // Gravado na hora, fora de transacao maior: o refresh token antigo ja nao vale.
                    await _unitOfWork.DadosBancariosEmpresa.Salvar(dados);
                    _logger.LogInformation("MercadoPago: autorizacao da empresa {EmpresaId} renovada ate {ExpiraEm}.", dados.EmpresaId, tokens.ExpiraEm);
                }
                catch (Exception ex)
                {
                    // A proxima rodada tenta de novo; vencido, a cobranca so muda de conta.
                    _logger.LogWarning(ex, "MercadoPago: nao foi possivel renovar a autorizacao da empresa {EmpresaId} (vence em {ExpiraEm}).",
                        dados.EmpresaId, dados.MercadoPagoTokenExpiraEm);
                }
            }
        }
    }
}
