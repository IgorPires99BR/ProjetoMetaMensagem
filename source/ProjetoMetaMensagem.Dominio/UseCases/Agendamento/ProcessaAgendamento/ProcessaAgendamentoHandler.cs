using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Chamado pela AgendamentoMensagemTarefa (HangFire, a cada 5 min) via mediator -- toda a
    // logica de negocio do processamento mora aqui, seguindo o mesmo desenho Command/Handler
    // dos demais casos de uso; a tarefa em si so dispara o comando.
    public class ProcessaAgendamentoHandler : IRequestHandler<ProcessaAgendamentoCommand, Response<ProcessaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ProcessaAgendamentoHandler> _logger;

        // Teto de agendamentos processados ao mesmo tempo numa mesma passada do job. Cada um
        // roda em seu proprio escopo de DI (conexao de banco propria -- IUnitOfWork nao e
        // thread-safe, ver ProcessarComEscopoProprio); o limite evita estourar o pool de
        // conexoes quando o numero de agendamentos devidos crescer.
        private const int GrauDeParalelismo = 4;

        private readonly object _travaResultado = new();

        public ProcessaAgendamentoHandler(
            IUnitOfWork unitOfWork, IServiceScopeFactory serviceScopeFactory, ILogger<ProcessaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<Response<ProcessaAgendamentoResult>> Handle(ProcessaAgendamentoCommand command)
        {
            var response = new Response<ProcessaAgendamentoResult>();

            var validator = new ProcessaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var resultado = new ProcessaAgendamentoResult();

            try
            {
                var pendentes = (await _unitOfWork.Agendamento.ObterPendentes(DateTime.Now)).ToList();

                if (pendentes.Count == 0)
                {
                    _logger.LogInformation("Varredura de agendamentos: nenhum pendente no momento.");
                    response.AddValue(resultado);
                    return response;
                }

                _logger.LogInformation("Varredura de agendamentos: {Total} pendente(s) de {Empresas} empresa(s).",
                    pendentes.Count, pendentes.Select(a => a.EmpresaId).Distinct().Count());

                // Paralelo (com teto) em vez de um foreach sequencial: cada agendamento e
                // independente (template e lista de contatos proprios), entao um disparo lento
                // pra Meta nao precisa segurar os outros agendamentos devidos na mesma passada.
                // A reserva via ReivindicarAgendamento continua sendo o que garante exclusividade
                // entre execucoes concorrentes (paralelismo aqui dentro, outra instancia da API,
                // ou uma passada atrasada) -- o grau de paralelismo so limita quanto rodamos ao
                // mesmo tempo dentro desta mesma passada.
                using var semaforo = new SemaphoreSlim(GrauDeParalelismo);
                var tarefas = pendentes.Select(agendamento => ProcessarComLimite(agendamento.Id, semaforo, resultado));
                await Task.WhenAll(tarefas);

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ProcessaAgendamentoHandler));
            }

            return response;
        }

        private async Task ProcessarComLimite(Guid agendamentoId, SemaphoreSlim semaforo, ProcessaAgendamentoResult resultado)
        {
            await semaforo.WaitAsync();
            try
            {
                await ProcessarComEscopoProprio(agendamentoId, resultado);
            }
            finally
            {
                semaforo.Release();
            }
        }

        // Escopo de DI proprio por agendamento: IUnitOfWork encapsula uma unica SqlConnection
        // (ver DbSession), entao rodar comandos concorrentes sobre a mesma instancia corromperia
        // os resultados -- cada tarefa paralela precisa da sua propria conexao/transacao, mesmo
        // padrao ja usado pelo CampanhaWorker (IServiceScopeFactory.CreateScope() por item).
        private async Task ProcessarComEscopoProprio(Guid agendamentoId, ProcessaAgendamentoResult resultadoAgregado)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            lock (_travaResultado) { resultadoAgregado.TotalAgendamentos++; }

            try
            {
                var agendamento = await unitOfWork.Agendamento.ObterPorId(agendamentoId, null);
                if (agendamento == null)
                {
                    // Excluido entre a varredura (escopo externo) e este processamento.
                    return;
                }

                // Reserva por prazo: se outra passada ja pegou este agendamento (deploy
                // sobreposto, execucao atrasada), pula sem reenviar.
                var prazoProcessamento = DateTime.Now.AddMinutes(10);
                if (!await unitOfWork.Agendamento.ReivindicarAgendamento(agendamento.Id, prazoProcessamento))
                {
                    _logger.LogInformation("Agendamento {AgendamentoId} ja estava sendo processado por outra execucao; pulando.",
                        agendamento.Id);
                    return;
                }

                await ProcessarUmAgendamento(unitOfWork, mediator, agendamento, resultadoAgregado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar agendamento {AgendamentoId}", agendamentoId);
            }
        }

        private async Task ProcessarUmAgendamento(
            IUnitOfWork unitOfWork, IMediator mediator, Entidades.Agendamento agendamento, ProcessaAgendamentoResult resultado)
        {
            var contatoIds = (await unitOfWork.Agendamento.ObterContatoIds(agendamento.Id)).Distinct().ToList();

            // Busca em lotes: o IN (@Ids) do ObterPorIds vira um parametro por id, e o SQL
            // Server corta em 2100 -- mesmo cuidado do CampanhaWorker.
            var contatos = new List<Entidades.Contato>();
            foreach (var lote in contatoIds.Chunk(1000))
            {
                contatos.AddRange(await unitOfWork.Contato.ObterPorIds(agendamento.EmpresaId, lote));
            }

            _logger.LogInformation("Agendamento {AgendamentoId} ({Nome}): {Total} contato(s) para disparo.",
                agendamento.Id, agendamento.Nome, contatos.Count);

            var template = await unitOfWork.Template.ObterPorIdEEmpresa(agendamento.TemplateId, agendamento.EmpresaId);

            var sucessos = 0;
            var falhas = 0;

            if (template == null)
            {
                _logger.LogWarning("Template {TemplateId} do agendamento {AgendamentoId} nao foi encontrado; nenhuma mensagem sera enviada.",
                    agendamento.TemplateId, agendamento.Id);
                falhas = contatos.Count;
            }
            else if (contatos.Count > 0)
            {
                // Disparo em lote (mesma estrutura usada pela tela de Disparos): um unico
                // envio pra todos os contatos deste agendamento, em vez de uma chamada por
                // contato -- menos round-trips e reaproveita o registro de HistoricoDisparo
                // que o handler do lote ja faz.
                var comandoLote = new EnviarMensagemTemplateMetaLoteCommand
                {
                    IdEmpresa = agendamento.EmpresaId,
                    EmpresaId = agendamento.EmpresaId,
                    TemplateId = agendamento.TemplateId,
                    NomeTemplate = template.NomeTemplate,
                    Idioma = template.Idioma ?? "pt_BR",
                    Telefones = contatos.Select(c => c.Telefone).ToList(),
                    ContatosIds = contatos.Select(c => c.Id.ToString()).ToList()
                };

                PreencherParametrosVariaveis(comandoLote, agendamento.Variaveis, contatos);

                var resultadoEnvio = await mediator.Send(comandoLote);

                if (resultadoEnvio is not null && !resultadoEnvio.HasValidations)
                {
                    sucessos = resultadoEnvio.Value.TotalSucesso;
                    falhas = resultadoEnvio.Value.TotalFalha;

                    foreach (var erro in resultadoEnvio.Value.RelatorioErros)
                    {
                        _logger.LogError("Agendamento {AgendamentoId}: falha ao enviar para {Telefone} -- {Motivo}",
                            agendamento.Id, erro.Key, erro.Value);
                    }

                    _logger.LogInformation("Agendamento {AgendamentoId} concluido: {Sucessos} sucesso(s), {Falhas} falha(s).",
                        agendamento.Id, sucessos, falhas);
                }
                else
                {
                    falhas = contatos.Count;
                    var motivo = resultadoEnvio is null
                        ? "O disparo em lote não retornou resposta."
                        : string.Join("; ", resultadoEnvio.Erros);
                    _logger.LogError("Agendamento {AgendamentoId}: falha ao disparar em lote -- {Motivo}", agendamento.Id, motivo);
                }
            }

            lock (_travaResultado)
            {
                resultado.TotalContatos += contatos.Count;
                resultado.Sucessos += sucessos;
                resultado.Falhas += falhas;
            }

            await unitOfWork.Agendamento.IncluirExecucao(new Entidades.AgendamentoExecucao
            {
                AgendamentoId = agendamento.Id,
                TotalContatos = contatos.Count,
                Sucessos = sucessos,
                Falhas = falhas,
                Status = falhas == 0 ? Entidades.AgendamentoExecucao.Concluida : Entidades.AgendamentoExecucao.Erro
            });

            var proximaExecucao = AgendamentoRecorrencia.CalcularProximaExecucao(
                agendamento.DataReferencia, agendamento.ProximaExecucao, agendamento.TipoRecorrencia,
                agendamento.DiasSemanaLista, agendamento.DiaDoMes);

            // Passou da data de termino: desativa em vez de continuar agendando alem do que
            // o cliente pediu.
            var aindaAtivo = !agendamento.DataFim.HasValue || proximaExecucao <= agendamento.DataFim.Value;

            await unitOfWork.Agendamento.FinalizarExecucao(agendamento.Id, proximaExecucao, aindaAtivo);
        }

        // Mesma logica da tela de Disparo em lote (DisparadorComponent.valorDaVariavel):
        // "nome"/"telefone" sao resolvidos por contato a cada execucao recorrente, "fixo" e o
        // mesmo texto pra todo mundo. Sem isto, o disparo em lote ia sem nenhum parametro e a
        // Meta recusava qualquer template com variavel no corpo.
        private static void PreencherParametrosVariaveis(
            EnviarMensagemTemplateMetaLoteCommand comandoLote,
            List<Entidades.AgendamentoVariavelDto> variaveis,
            List<Entidades.Contato> contatos)
        {
            if (variaveis == null || variaveis.Count == 0) return;

            comandoLote.ParametrosBody = variaveis
                .Select(v => v.Origem == "fixo" ? (v.ValorFixo ?? string.Empty).Trim() : string.Empty)
                .ToList();

            var personalizado = variaveis.Any(v => v.Origem != "fixo");
            if (!personalizado) return;

            foreach (var contato in contatos)
            {
                comandoLote.ParametrosBodyPorTelefone[contato.Telefone] = variaveis
                    .Select(v => ResolverValorVariavel(v, contato))
                    .ToList();
            }
        }

        private static string ResolverValorVariavel(Entidades.AgendamentoVariavelDto variavel, Entidades.Contato contato)
        {
            return variavel.Origem switch
            {
                "nome" => contato.Nome ?? string.Empty,
                "telefone" => contato.Telefone ?? string.Empty,
                _ => (variavel.ValorFixo ?? string.Empty).Trim()
            };
        }
    }
}
