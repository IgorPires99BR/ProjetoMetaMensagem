using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;

namespace ProjetoMetaMensagem.Servico.Campanha
{
    public class CampanhaWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CampanhaWorker> _logger;

        public CampanhaWorker(IServiceScopeFactory serviceScopeFactory, ILogger<CampanhaWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CampanhaWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var campanhasPendentes = await unitOfWork.Campanha.ObterPendentes();

                        foreach (var campanha in campanhasPendentes)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            try
                            {
                                _logger.LogInformation("Processando campanha {CampanhaId}: {Nome}", campanha.Id, campanha.Nome);

                                // Escopo null: o worker roda sem usuario logado e precisa
                                // atualizar campanhas de todas as empresas.
                                await unitOfWork.Campanha.AtualizarStatus(campanha.Id, "PROCESSANDO", null);

                                var vinculos = (await unitOfWork.Campanha.ObterContatosPorCampanha(campanha.Id)).ToList();
                                var todosContatos = await unitOfWork.Contato.Obter();
                                var mapaContatos = todosContatos.ToDictionary(c => c.Id);

                                foreach (var vinculo in vinculos)
                                {
                                    if (stoppingToken.IsCancellationRequested)
                                        break;

                                    try
                                    {
                                        if (!mapaContatos.TryGetValue(vinculo.ContatoId, out var contato))
                                        {
                                            _logger.LogWarning("Contato {ContatoId} nao encontrado para campanha {CampanhaId}", vinculo.ContatoId, campanha.Id);
                                            vinculo.Processado = true;
                                            vinculo.Sucesso = false;
                                            vinculo.MensagemErro = "Contato nao encontrado";
                                            continue;
                                        }

                                        var enviarCommand = new EnviarMensagemTemplateMetaCommand
                                        {
                                            IdEmpresa = campanha.EmpresaId,
                                            ContatoId = vinculo.ContatoId,
                                            Telefone = contato.Telefone,
                                            TemplateId = campanha.TemplateId
                                        };

                                        var resultado = await mediator.Send(enviarCommand);

                                        if (resultado is not null && !resultado.HasValidations)
                                        {
                                            vinculo.Processado = true;
                                            vinculo.Sucesso = true;
                                        }
                                        else
                                        {
                                            vinculo.Processado = true;
                                            vinculo.Sucesso = false;
                                            // resultado nulo tambem cai aqui: usar resultado.Erros direto
                                            // estourava NullReference e a causa real virava a mensagem
                                            // generica do catch abaixo.
                                            vinculo.MensagemErro = resultado is null
                                                ? "O disparo nao retornou resposta."
                                                : string.Join("; ", resultado.Erros);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Erro ao enviar mensagem para contato {ContatoId} na campanha {CampanhaId}", vinculo.ContatoId, campanha.Id);
                                        vinculo.Processado = true;
                                        vinculo.Sucesso = false;
                                        vinculo.MensagemErro = ex.Message;
                                    }
                                }

                                var total = vinculos.Count;
                                var processados = vinculos.Count(v => v.Processado);

                                await unitOfWork.Campanha.AtualizarStatus(campanha.Id, "CONCLUIDA", null);

                                _logger.LogInformation("Campanha {CampanhaId} concluida com {Total}/{Processados} processados",
                                    campanha.Id, total, processados);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Erro ao processar campanha {CampanhaId}", campanha.Id);
                                await unitOfWork.Campanha.AtualizarStatus(campanha.Id, "ERRO", null);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro geral no CampanhaWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            _logger.LogInformation("CampanhaWorker parado.");
        }
    }
}
