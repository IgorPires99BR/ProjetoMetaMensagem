using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System.Diagnostics;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Health
{
    // Diz se a API esta de pe e se ela consegue falar com o banco.
    //
    // Existe porque, quando o banco ficou inacessivel em producao, todo endpoint respondia 500
    // com a mensagem generica (proposital, pra nao vazar detalhe tecnico ao usuario) e nao havia
    // como distinguir "a aplicacao caiu" de "o banco nao responde" sem abrir o log do servidor.
    //
    // NAO expoe mensagem de excecao, servidor, base nem credencial: so o resultado e o tempo.
    [ApiController]
    [Route("api/health")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        // IUnitOfWork NAO pode ser injetado no construtor: DbSession abre a conexao SQL de
        // forma sincrona no proprio construtor, entao a excecao estouraria na resolucao de DI
        // do controller, antes do try/catch abaixo rodar (era exatamente o que este endpoint
        // deveria diagnosticar). Resolvendo pelo IServiceProvider dentro do try, o catch consegue
        // capturar a falha de conexao normalmente.
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IServiceProvider serviceProvider, ILogger<HealthController> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Verificar()
        {
            var relogio = Stopwatch.StartNew();
            string estadoBanco;
            var tudoOk = true;

            try
            {
                // Consulta trivial: so confirma que a conexao abre e responde.
                var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.Usuario.ObterPorEmail("health-check@contactsolution.local");
                estadoBanco = "ok";
            }
            catch (Exception ex)
            {
                tudoOk = false;
                estadoBanco = "indisponivel";
                // O detalhe fica so no log do servidor.
                _logger.LogError(ex, "Health check: falha ao consultar o banco de dados");
            }

            relogio.Stop();

            var resposta = new
            {
                api = "ok",
                banco = estadoBanco,
                latenciaMs = relogio.ElapsedMilliseconds,
                ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "desconhecido",
                versao = typeof(HealthController).Assembly.GetName().Version?.ToString(),
                horaServidor = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 503 quando o banco nao responde, pra monitoramento externo conseguir alertar.
            return tudoOk ? Ok(resposta) : StatusCode(503, resposta);
        }
    }
}
