using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConcluiConexaoMercadoPago;
using ProjetoMetaMensagem.Servico.Configuration;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Retorno do OAuth do Mercado Pago (redirect_uri cadastrada no app: MercadoPago:UrlRetornoOAuth).
    // Quem chega e o navegador do admin da empresa, sem o token da API: a empresa vem no state.
    // Sempre termina redirecionando para a tela de Empresas, com o resultado na query string.
    public class MercadoPagoOAuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly MercadoPagoConfiguration _config;
        private readonly ILogger<MercadoPagoOAuthController> _logger;

        public MercadoPagoOAuthController(IMediator mediator, IOptions<MercadoPagoConfiguration> config, ILogger<MercadoPagoOAuthController> logger)
        {
            _mediator = mediator;
            _config = config.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("api/mercadopago/oauth/retorno")]
        public async Task<IActionResult> Retorno([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            // A empresa recusou a autorizacao no Mercado Pago.
            if (!string.IsNullOrWhiteSpace(error) || string.IsNullOrWhiteSpace(code))
                return Redirect(Tela("erro", "A autorização no Mercado Pago foi cancelada."));

            try
            {
                var resultado = await _mediator.Send(new ConcluiConexaoMercadoPagoCommand { Code = code, State = state });
                if (resultado.HasValidations || resultado.Value == null)
                    return Redirect(Tela("erro", string.Join(" ", resultado.Erros.Select(e => e.Mensagem))));

                return Redirect(Tela("conectado", null, resultado.Value.EmpresaId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MercadoPago OAuth: falha no retorno da autorizacao");
                return Redirect(Tela("erro", "Não foi possível concluir a conexão com o Mercado Pago. Tente novamente."));
            }
        }

        private string Tela(string resultado, string? mensagem, Guid? empresaId = null)
        {
            var url = $"{_config.UrlTelaEmpresas}?mercadopago={resultado}";
            if (empresaId.HasValue) url += $"&empresa={empresaId.Value}";
            if (!string.IsNullOrWhiteSpace(mensagem)) url += $"&mensagem={Uri.EscapeDataString(mensagem)}";
            return url;
        }
    }
}
