using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Servicos;

namespace ProjetoMetaMensagem.Servico.DisparoLote
{
    // Consome a fila do disparo em massa. Poucos consumidores de proposito: e isso que limita o
    // ritmo de chamadas ao provedor de Pix e a Meta. Cada contato usa um escopo proprio, porque a
    // conexao do DbSession nao pode ser dividida entre tarefas.
    public class DisparoLoteWorker : BackgroundService
    {
        private const int Consumidores = 3;
        private static readonly TimeSpan EsperaSemItem = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan EsperaInstavel = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan EsperaAposErro = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan IntervaloDaRecuperacao = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DisparoLoteWorker> _logger;

        public DisparoLoteWorker(IServiceScopeFactory scopeFactory, ILogger<DisparoLoteWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DisparoLoteWorker iniciado.");

            var tarefas = Enumerable.Range(0, Consumidores)
                .Select(_ => ConsumirAsync(stoppingToken))
                .Append(RecuperarAsync(stoppingToken));

            await Task.WhenAll(tarefas);

            _logger.LogInformation("DisparoLoteWorker parado.");
        }

        private async Task ConsumirAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan espera;
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var processador = scope.ServiceProvider.GetRequiredService<IProcessadorDisparoLote>();

                    espera = await processador.ProcessarProximoAsync() switch
                    {
                        ResultadoProcessamentoDisparo.SemItem => EsperaSemItem,
                        ResultadoProcessamentoDisparo.Instavel => EsperaInstavel,
                        _ => TimeSpan.Zero
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DisparoLoteWorker: falha ao buscar o proximo item da fila.");
                    espera = EsperaAposErro;
                }

                if (espera > TimeSpan.Zero && !await EsperarAsync(espera, stoppingToken)) return;
            }
        }

        private async Task RecuperarAsync(CancellationToken stoppingToken)
        {
            while (await EsperarAsync(IntervaloDaRecuperacao, stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IProcessadorDisparoLote>().ResolverInterrompidosAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DisparoLoteWorker: falha ao recuperar itens interrompidos.");
                }
            }
        }

        private static async Task<bool> EsperarAsync(TimeSpan espera, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(espera, stoppingToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
