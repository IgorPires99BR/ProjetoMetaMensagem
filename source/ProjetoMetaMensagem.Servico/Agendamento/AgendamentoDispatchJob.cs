using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Agendamento
{
    // Chamado pelo HangFire como job recorrente (RecurringJob.AddOrUpdate, ver Program.cs) --
    // nao herda de BackgroundService nem tem loop proprio, quem controla o intervalo e o
    // HangFire. Repositorios sao Scoped e este job roda fora do pipeline HTTP, entao cada
    // execucao abre seu proprio escopo de DI (mesmo motivo do CampanhaWorker).
    public class AgendamentoDispatchJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AgendamentoFileLogger _fileLogger;
        private readonly ILogger<AgendamentoDispatchJob> _logger;

        public AgendamentoDispatchJob(
            IServiceScopeFactory serviceScopeFactory,
            AgendamentoFileLogger fileLogger,
            ILogger<AgendamentoDispatchJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _fileLogger = fileLogger;
            _logger = logger;
        }

        public async Task ExecutarAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var agora = DateTime.Now;
            var pendentes = await unitOfWork.Agendamento.ObterPendentes(agora);

            foreach (var agendamento in pendentes)
            {
                try
                {
                    // Reserva por prazo: se outra passada ja pegou este agendamento (deploy
                    // sobreposto, job atrasado), pula sem reenviar.
                    var prazoProcessamento = DateTime.Now.AddMinutes(10);
                    if (!await unitOfWork.Agendamento.ReivindicarAgendamento(agendamento.Id, prazoProcessamento))
                    {
                        continue;
                    }

                    await ProcessarAgendamento(agendamento, mediator, unitOfWork);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar agendamento {AgendamentoId}", agendamento.Id);
                    _fileLogger.ErroInesperado(agendamento.Id, ex);
                }
            }
        }

        private async Task ProcessarAgendamento(Dominio.Entidades.Agendamento agendamento, IMediator mediator, IUnitOfWork unitOfWork)
        {
            var contatoIds = (await unitOfWork.Agendamento.ObterContatoIds(agendamento.Id)).Distinct().ToList();

            // Busca em lotes: o IN (@Ids) do ObterPorIds vira um parametro por id, e o SQL
            // Server corta em 2100 -- mesmo cuidado do CampanhaWorker.
            var contatos = new List<Dominio.Entidades.Contato>();
            foreach (var lote in contatoIds.Chunk(1000))
            {
                contatos.AddRange(await unitOfWork.Contato.ObterPorIds(agendamento.EmpresaId, lote));
            }

            _fileLogger.InicioExecucao(agendamento.Id, agendamento.Nome, contatos.Count);
            _logger.LogInformation("Processando agendamento {AgendamentoId}: {Nome} ({Total} contatos)",
                agendamento.Id, agendamento.Nome, contatos.Count);

            // Resolvido uma vez por execucao (mesmo template pra todo mundo do agendamento).
            // O Command so leva o Id -- sem preencher NomeTemplate/Idioma aqui, o Handler manda
            // pra Meta um payload de template sem "name" e ela recusa toda mensagem.
            var template = await unitOfWork.Template.ObterPorIdEEmpresa(agendamento.TemplateId, agendamento.EmpresaId);

            var sucessos = 0;
            var falhas = 0;

            if (template == null)
            {
                _logger.LogWarning("Template {TemplateId} do agendamento {AgendamentoId} nao foi encontrado; nenhuma mensagem sera enviada.",
                    agendamento.TemplateId, agendamento.Id);
                foreach (var contato in contatos)
                {
                    falhas++;
                    _fileLogger.Falha(agendamento.Id, contato.Id, contato.Telefone, "Modelo de mensagem do agendamento não foi encontrado.");
                }
            }
            else
            foreach (var contato in contatos)
            {
                try
                {
                    var comando = new EnviarMensagemTemplateMetaCommand
                    {
                        IdEmpresa = agendamento.EmpresaId,
                        EmpresaId = agendamento.EmpresaId,
                        ContatoId = contato.Id,
                        Telefone = contato.Telefone,
                        TemplateId = agendamento.TemplateId,
                        NomeTemplate = template.NomeTemplate,
                        Idioma = template.Idioma ?? "pt_BR"
                    };

                    var resultado = await mediator.Send(comando);

                    if (resultado is not null && !resultado.HasValidations)
                    {
                        sucessos++;
                        _fileLogger.Sucesso(agendamento.Id, contato.Id, contato.Telefone);
                    }
                    else
                    {
                        falhas++;
                        var motivo = resultado is null
                            ? "O disparo não retornou resposta."
                            : string.Join("; ", resultado.Erros);
                        _fileLogger.Falha(agendamento.Id, contato.Id, contato.Telefone, motivo);
                    }
                }
                catch (Exception ex)
                {
                    falhas++;
                    _logger.LogError(ex, "Erro ao enviar mensagem para contato {ContatoId} no agendamento {AgendamentoId}",
                        contato.Id, agendamento.Id);
                    _fileLogger.Falha(agendamento.Id, contato.Id, contato.Telefone, ex.Message);
                }
            }

            _fileLogger.FimExecucao(agendamento.Id, sucessos, falhas);

            await unitOfWork.Agendamento.IncluirExecucao(new Dominio.Entidades.AgendamentoExecucao
            {
                AgendamentoId = agendamento.Id,
                TotalContatos = contatos.Count,
                Sucessos = sucessos,
                Falhas = falhas,
                Status = falhas == 0 ? Dominio.Entidades.AgendamentoExecucao.Concluida : Dominio.Entidades.AgendamentoExecucao.Erro
            });

            var proximaExecucao = AgendamentoRecorrencia.CalcularProximaExecucao(
                agendamento.DataInicio, agendamento.ProximaExecucao, agendamento.TipoRecorrencia);

            // Passou da data de termino: desativa em vez de continuar agendando alem do que
            // o cliente pediu.
            var aindaAtivo = !agendamento.DataFim.HasValue || proximaExecucao <= agendamento.DataFim.Value;

            await unitOfWork.Agendamento.FinalizarExecucao(agendamento.Id, proximaExecucao, aindaAtivo);
        }
    }
}
