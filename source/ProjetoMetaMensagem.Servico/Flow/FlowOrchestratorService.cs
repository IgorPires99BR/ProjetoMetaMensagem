using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Servico.Flow
{
    public class FlowOrchestratorService : IFlowOrchestratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly INotificadorChat _notificadorChat;
        private readonly ILogger<FlowOrchestratorService> _logger;
        private readonly ProjetoMetaMensagem.Servico.Configuration.PlanosConfiguration _planosConfig;

        public FlowOrchestratorService(
            IUnitOfWork unitOfWork,
            IMetaService metaService,
            ILogger<FlowOrchestratorService> logger,
            INotificadorChat notificadorChat,
            Microsoft.Extensions.Options.IOptions<ProjetoMetaMensagem.Servico.Configuration.PlanosConfiguration> planosConfig)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _notificadorChat = notificadorChat;
            _logger = logger;
            _planosConfig = planosConfig.Value;
        }

        public async Task<FlowOrchestrationResult> ProcessarMensagem(
            Guid empresaId, Guid contatoId, string celular, string mensagem, string? phoneNumberIdOrigem = null, Guid? numeroId = null)
            => await ProcessarMensagem(empresaId, contatoId, celular, mensagem, phoneNumberIdOrigem, numeroId, jaTentouNovamente: false);

        // "jaTentouNovamente" so existe pra esse metodo poder chamar a si mesmo uma unica vez
        // apos perder a corrida de criar a conversa (ver catch abaixo) -- nunca passar true de
        // fora, e o parametro fica de fora da assinatura publica de proposito.
        private async Task<FlowOrchestrationResult> ProcessarMensagem(
            Guid empresaId, Guid contatoId, string celular, string mensagem, string? phoneNumberIdOrigem, Guid? numeroId, bool jaTentouNovamente)
        {
            var resultado = new FlowOrchestrationResult();

            // Reserva ANTES de abrir a transacao: dentro dela a reserva ficaria invisivel pras
            // outras requisicoes, que sao justamente quem precisa enxergar. Cliente que manda
            // tres mensagens seguidas gera tres requisicoes -- so a que reservar responde, as
            // outras saem aqui sem enviar nada.
            var reserva = await _unitOfWork.ConversationState.TentarReservarProcessamento(empresaId, contatoId, SegundosDeReserva);

            if (reserva == ResultadoReserva.JaEmProcessamento)
            {
                _logger.LogInformation(
                    "Mensagem de {ContatoId} ignorada pelo Flow: outra mensagem do mesmo contato ja esta sendo processada",
                    contatoId);

                resultado.Sucesso = false;
                resultado.Mensagem = "Outra mensagem deste contato ja esta sendo processada.";
                return resultado;
            }

            var precisaLiberar = reserva == ResultadoReserva.Reservada;

            try
            {
                _unitOfWork.BeginTransaction();

                // 1. Verifica se ja existe uma conversa ativa para este contato
                var estadoAtual = await _unitOfWork.ConversationState.ObterAtivaParaAtualizacao(empresaId, contatoId);

                // Vendedor assumiu essa conversa manualmente pelo chat -- o flow fica pausado
                // (etapa/variaveis preservadas) ate ser devolvido ao bot.
                if (estadoAtual != null && estadoAtual.AssumidoPorUsuarioId != null)
                {
                    _unitOfWork.Commit();
                    resultado.Sucesso = false;
                    resultado.Mensagem = "Conversa assumida manualmente por um vendedor; flow nao processado.";
                    return resultado;
                }

                // O bot ja desistiu desta conversa e avisou que chamaria uma pessoa. Sem esta
                // saida ele reavaliaria cada mensagem seguinte e repetiria o mesmo aviso.
                if (estadoAtual != null && estadoAtual.AguardandoAtendente)
                {
                    _unitOfWork.Commit();
                    resultado.Sucesso = false;
                    resultado.Mensagem = "Conversa aguardando atendente; flow nao processado.";
                    return resultado;
                }

                if (estadoAtual == null)
                {
                    // 2. Nao ha conversa ativa -> busca um flow cujo gatilho corresponda a mensagem.
                    // Inclui os flows genericos da empresa (NumeroId nulo) e os especificos do
                    // numero que recebeu a mensagem; entre os que baterem o gatilho, o especifico
                    // do numero tem prioridade sobre o generico (OrderBy false primeiro: NumeroId
                    // != null vira "false" e ordena antes de "true"/generico).
                    var flows = await _unitOfWork.Flow.ObterTodosPorEmpresaENumero(empresaId, numeroId);

                    IEnumerable<string> GatilhosDe(Dominio.Entidades.Flow f) =>
                        (f.GatilhoInicial ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var flowAtivado = flows
                        .Where(f =>
                            f.Ativo &&
                            !string.IsNullOrEmpty(f.GatilhoInicial) &&
                            GatilhosDe(f).Any(g => mensagem.Trim().Equals(g, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(f => f.NumeroId == null)
                        .FirstOrDefault();

                    // Nenhum gatilho exato bateu: cai no flow curinga, se a empresa tiver um.
                    //
                    // O gatilho tinha que ser IGUAL a mensagem pra ativar um flow, o que quebra
                    // justamente quem chega de anuncio Click-to-WhatsApp: o WhatsApp abre a
                    // conversa com um texto sugerido, mas a pessoa apaga e escreve o que quer --
                    // e aí ninguem respondia. Um flow com gatilho "*" atende qualquer primeira
                    // mensagem, e perde pros gatilhos exatos, que continuam tendo prioridade.
                    if (flowAtivado == null)
                    {
                        flowAtivado = flows
                            .Where(f => f.Ativo && GatilhosDe(f).Any(g => g == "*"))
                            .OrderBy(f => f.NumeroId == null)
                            .FirstOrDefault();

                        if (flowAtivado != null)
                        {
                            _logger.LogInformation(
                                "Flow curinga {FlowId} atendeu a mensagem que nao casou com nenhum gatilho exato",
                                flowAtivado.Id);
                        }
                    }

                    if (flowAtivado == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Nenhum flow encontrado para esta mensagem.";
                        return resultado;
                    }

                    // 3. Obtem a etapa inicial do flow
                    var etapaInicial = await _unitOfWork.Flow.ObterEtapaInicial(flowAtivado.Id);
                    if (etapaInicial == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Flow nao possui etapa inicial configurada.";
                        return resultado;
                    }

                    // 4. Cria novo estado de conversa
                    estadoAtual = new ConversationState
                    {
                        EmpresaId = empresaId,
                        ContatoId = contatoId,
                        FlowId = flowAtivado.Id,
                        EtapaAtualId = etapaInicial.Id,
                        Variaveis = "{}",
                        Finalizado = false
                    };

                    await _unitOfWork.ConversationState.Incluir(estadoAtual);

                    // 5. Executa a etapa inicial (deve ser uma mensagem de boas-vindas) e segue
                    // pelas etapas que nao esperam resposta -- ver EncadearEtapasSemRespostaAsync.
                    await ExecutarEtapa(etapaInicial, null, estadoAtual, empresaId, celular, phoneNumberIdOrigem);
                    var etapaFinalDaVez = await EncadearEtapasSemRespostaAsync(
                        etapaInicial, estadoAtual, empresaId, celular, phoneNumberIdOrigem);

                    estadoAtual.EtapaAtualId = etapaFinalDaVez.Id;
                    estadoAtual.DataAtualizacao = DateTime.Now;
                    await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                    resultado.Sucesso = true;
                    resultado.FlowId = flowAtivado.Id;
                    resultado.EtapaId = etapaFinalDaVez.Id;
                    resultado.Mensagem = $"Flow '{flowAtivado.Nome}' iniciado.";
                }
                else
                {
                    // 6. Conversa ja ativa -> avanca para a proxima etapa baseado na resposta

                    // EtapaAtualId e NULL-avel no banco: conversa nao finalizada sem etapa e estado
                    // inconsistente (dado legado/migracao). Finaliza em vez de estourar -- era o unico
                    // ponto do fluxo que subia excecao ate o handler do webhook em vez de degradar.
                    if (estadoAtual.EtapaAtualId == null)
                    {
                        _logger.LogWarning(
                            "EstadoConversa {EstadoId} do contato {ContatoId} esta ativo sem EtapaAtualId; finalizando a conversa.",
                            estadoAtual.Id, contatoId);

                        estadoAtual.Finalizado = true;
                        estadoAtual.DataAtualizacao = DateTime.Now;
                        await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.FlowFinalizado = true;
                        resultado.Mensagem = "Conversa sem etapa atual; flow finalizado.";
                        return resultado;
                    }

                    var etapaAtualId = estadoAtual.EtapaAtualId.Value;
                    var etapaAtual = await _unitOfWork.Flow.ObterEtapaPorId(etapaAtualId);
                    if (etapaAtual == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Etapa atual nao encontrada.";
                        return resultado;
                    }

                    // 6.1 Etapa de botao onde a resposta nao casa com nenhum deles: o cliente
                    // desconversou ou digitou outra coisa. Reperguntar eternamente irrita e
                    // perde o lead, entao na segunda vez a conversa vai pra uma pessoa.
                    if (TemBotoes(etapaAtual) && !RespostaCasaComAlgumBotao(mensagem, etapaAtual))
                    {
                        estadoAtual.TentativasNaEtapa++;
                        estadoAtual.DataAtualizacao = DateTime.Now;

                        var desistir = estadoAtual.TentativasNaEtapa >= TentativasAntesDeChamarHumano;

                        if (desistir)
                        {
                            estadoAtual.AguardandoAtendente = true;
                            await EnviarTextoAsync(empresaId, contatoId, celular, phoneNumberIdOrigem,
                                "Acho melhor uma pessoa te ajudar com isso 🙋\n\n" +
                                "Já avisei o time — em breve alguém entra na conversa por aqui.");

                            _logger.LogInformation(
                                "Conversa do contato {ContatoId} entregue a um atendente apos {Tentativas} respostas fora do esperado",
                                contatoId, estadoAtual.TentativasNaEtapa);
                        }
                        else
                        {
                            // Repergunta a MESMA etapa (com os botoes), so que avisando que nao
                            // entendeu -- reiniciar o flow faria a pessoa repetir tudo de novo.
                            await ExecutarEtapa(etapaAtual, estadoAtual.Variaveis, estadoAtual, empresaId, celular, phoneNumberIdOrigem,
                                prefixo: "Não entendi 😅 Toque em uma das opções abaixo:\n\n");
                        }

                        await _unitOfWork.ConversationState.Atualizar(estadoAtual);
                        _unitOfWork.Commit();

                        resultado.Sucesso = true;
                        resultado.Mensagem = desistir
                            ? "Conversa entregue a um atendente."
                            : "Resposta fora do esperado; etapa reenviada.";
                        return resultado;
                    }

                    // Acertou: zera a contagem pra uma confusao futura comecar do zero.
                    estadoAtual.TentativasNaEtapa = 0;

                    // 7. Se a etapa atual captura input, salva a resposta como variavel
                    if (etapaAtual.NomeEtapa == "Capturar Input")
                    {
                        var variavelNome = ObterVariavelSaida(etapaAtual);
                        if (!string.IsNullOrEmpty(variavelNome))
                        {
                            var variaveis = JsonConvert.DeserializeObject<Dictionary<string, string>>(estadoAtual.Variaveis ?? "{}")
                                           ?? new Dictionary<string, string>();
                            variaveis[variavelNome] = mensagem;

                            // Etapa de escolha de plano: alem de guardar a resposta, resolve na
                            // hora o link de pagamento certo, pra proxima etapa so precisar
                            // mandar "{{link_pagamento}}".
                            //
                            // Confere se os botoes sao mesmo de plano em vez de aceitar qualquer
                            // etapa com dois botoes: uma pergunta como "O que fazem?/Quanto
                            // custa?" tambem tem dois botoes e estava gravando um link de plano
                            // que ninguem escolheu.
                            if (EhEscolhaDePlano(etapaAtual))
                            {
                                variaveis["link_pagamento"] = ResolverLinkPagamento(mensagem);
                            }

                            estadoAtual.Variaveis = JsonConvert.SerializeObject(variaveis);
                        }
                    }

                    // 8. Ramificacao: etapa com dois botoes pode ter uma segunda saida. Se a
                    // resposta casa com o Botao2 e existe caminho B, salta pra ele -- e assim
                    // que "Quanto custa?" pula a explicacao e cai direto nos planos.
                    FlowEtapa? proximaEtapa = null;

                    if (etapaAtual.ProximaEtapaIdB.HasValue && RespostaCasaComBotao(mensagem, etapaAtual.Botao2))
                    {
                        proximaEtapa = await _unitOfWork.Flow.ObterEtapaPorId(etapaAtual.ProximaEtapaIdB.Value);
                    }

                    proximaEtapa ??= await _unitOfWork.Flow.ObterProximaEtapa(etapaAtualId, mensagem);

                    if (proximaEtapa == null)
                    {
                        // Nenhuma etapa correspondeu -> finaliza ou reenvia a mesma etapa
                        if (etapaAtual.NomeEtapa == "Capturar Input")
                        {
                            // Se estava capturando input, tenta avancar com "Qualquer_Resposta"
                            proximaEtapa = await _unitOfWork.Flow.ObterProximaEtapa(etapaAtualId, "Qualquer_Resposta");
                        }
                        else
                        {
                            // Etapa de mensagem recebe GatilhoResposta "Avancar" na criacao, que
                            // nunca casa com o texto do cliente. Sem esta rede, uma conversa que
                            // parou numa mensagem era FINALIZADA na resposta seguinte -- o flow
                            // morria na saudacao.
                            proximaEtapa = await _unitOfWork.Flow.ObterProximaEtapa(etapaAtualId, "Avancar");
                        }

                        if (proximaEtapa == null)
                        {
                            estadoAtual.Finalizado = true;
                            estadoAtual.DataAtualizacao = DateTime.Now;
                            await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                            _unitOfWork.Commit();
                            resultado.Sucesso = true;
                            resultado.FlowFinalizado = true;
                            resultado.Mensagem = "Flow finalizado.";
                            return resultado;
                        }
                    }

                    // 9. Executa a nova etapa e as seguintes que nao esperam resposta
                    await ExecutarEtapa(proximaEtapa, estadoAtual.Variaveis, estadoAtual, empresaId, celular, phoneNumberIdOrigem);
                    proximaEtapa = await EncadearEtapasSemRespostaAsync(
                        proximaEtapa, estadoAtual, empresaId, celular, phoneNumberIdOrigem);

                    // 10. Atualiza o estado da conversa
                    estadoAtual.EtapaAtualId = proximaEtapa.Id;
                    estadoAtual.DataAtualizacao = DateTime.Now;
                    await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                    resultado.Sucesso = true;
                    resultado.FlowId = estadoAtual.FlowId;
                    resultado.EtapaId = proximaEtapa.Id;
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                // Duas mensagens do mesmo cliente quase juntas podem cair em requisicoes
                // concorrentes que leem "nenhuma conversa ativa" ao mesmo tempo -- so uma
                // consegue criar a EstadoConversa (UX_EstadoConversa_Ativa, BD/34), a outra
                // recebe erro de constraint aqui. Em vez de derrubar a mensagem do cliente que
                // perdeu a corrida, tenta de novo uma vez: agora a conversa ja existe, entao
                // cai no caminho normal de "conversa ja ativa" e a resposta sai do mesmo jeito.
                if (!jaTentouNovamente && EhViolacaoDeConversaDuplicada(ex))
                {
                    _logger.LogInformation(
                        "Corrida ao criar EstadoConversa para o contato {ContatoId} -- reprocessando contra a conversa que venceu",
                        contatoId);
                    return await ProcessarMensagem(empresaId, contatoId, celular, mensagem, phoneNumberIdOrigem, numeroId, jaTentouNovamente: true);
                }

                throw;
            }
            finally
            {
                // Libera a reserva aconteca o que acontecer. Se falhar aqui, a reserva ainda
                // vence sozinha pelo prazo -- por isso a falha e so registrada, nunca propagada:
                // ela nao pode virar o erro que o chamador ve no lugar do erro de verdade.
                if (precisaLiberar)
                {
                    try { await _unitOfWork.ConversationState.LiberarProcessamento(empresaId, contatoId); }
                    catch (Exception exLiberar)
                    {
                        _logger.LogError(exLiberar, "Falha ao liberar a reserva da conversa do contato {ContatoId}", contatoId);
                    }
                }
            }

            return resultado;
        }

        // Quanto tempo uma conversa fica reservada enquanto o Flow processa uma mensagem. Precisa
        // ser maior que o tempo normal de processamento (envio pra Meta incluso) e pequeno o
        // suficiente pra nao segurar a conversa se algo morrer no meio.
        private const int SegundosDeReserva = 20;

        // So conta como escolha de plano quando os dois botoes nomeiam planos de verdade.
        private static bool EhEscolhaDePlano(FlowEtapa etapa)
        {
            bool NomeiaPlano(string? texto) =>
                !string.IsNullOrWhiteSpace(texto) &&
                (texto.Contains("starter", StringComparison.OrdinalIgnoreCase) ||
                 texto.Contains("pro", StringComparison.OrdinalIgnoreCase));

            return NomeiaPlano(etapa.Botao1) && NomeiaPlano(etapa.Botao2);
        }

        // Quantas respostas fora do esperado o bot tolera antes de chamar uma pessoa. Duas, e
        // nao tres: na terceira insistencia a pessoa ja desistiu, e na segunda ainda da pra
        // salvar o atendimento.
        private const int TentativasAntesDeChamarHumano = 2;

        private static bool TemBotoes(FlowEtapa etapa) =>
            !string.IsNullOrWhiteSpace(etapa.Botao1) && !string.IsNullOrWhiteSpace(etapa.Botao2);

        private static bool RespostaCasaComAlgumBotao(string resposta, FlowEtapa etapa) =>
            RespostaCasaComBotao(resposta, etapa.Botao1) || RespostaCasaComBotao(resposta, etapa.Botao2);

        // Envia um texto avulso pro cliente (fora do conteudo de uma etapa) e registra no
        // historico, pra mensagem aparecer no Chats como qualquer outra do bot.
        private async Task EnviarTextoAsync(Guid empresaId, Guid contatoId, string celular, string? phoneNumberIdOrigem, string texto)
        {
            var token = await _unitOfWork.Empresa.ObterMetaAccessToken(empresaId);
            var phoneNumberId = phoneNumberIdOrigem ?? await _unitOfWork.Empresa.ObterPhoneNumberId(empresaId);

            var wamid = await _metaService.EnviarTextoLivreAsync(celular, texto, token!, phoneNumberId!);

            await _unitOfWork.HistoricoDisparo.Incluir(new HistoricoDisparo
            {
                EmpresaId = empresaId,
                ContatoId = contatoId,
                TipoDisparo = "Flow",
                Conteudo = texto,
                WamidMeta = wamid,
                DataEnvio = DateTime.Now
            });

            await _notificadorChat.NotificarMensagemEnviadaAsync(empresaId, contatoId, texto, wamid);
        }

        // O cliente pode tocar o botao (chega o titulo exato) ou digitar algo parecido. Compara
        // sem diferenciar maiuscula/acento de espaco sobrando, senao quem digita "quanto custa"
        // em vez de tocar o botao cai no caminho errado.
        private static bool RespostaCasaComBotao(string resposta, string? botao)
        {
            if (string.IsNullOrWhiteSpace(botao)) return false;

            return string.Equals(resposta?.Trim(), botao.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // So a violacao do indice de conversa ativa justifica reprocessar. Antes bastava ser
        // "chave duplicada" em qualquer tabela: um wamid repetido em HistoricoDisparo, por
        // exemplo, fazia o flow reprocessar e mandar a mesma mensagem de novo pro cliente.
        private const string IndiceDeConversaAtiva = "UX_EstadoConversa_Ativa";

        private static bool EhViolacaoDeConversaDuplicada(Exception ex)
        {
            var atual = ex;
            while (atual != null)
            {
                // 2601 = violacao de indice unico, 2627 = violacao de constraint unica --
                // os dois numeros que o SQL Server usa pra essa mesma familia de erro.
                if (atual is Microsoft.Data.SqlClient.SqlException sql
                    && (sql.Number == 2601 || sql.Number == 2627)
                    && sql.Message.Contains(IndiceDeConversaAtiva, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                atual = atual.InnerException;
            }

            return false;
        }

        // Etapa de "Mensagem" e informativa: ela nao espera resposta, entao o flow deve seguir
        // na hora para a proxima -- e o que faz a saudacao e a primeira pergunta chegarem juntas.
        // Antes, o flow enviava so a saudacao e parava; quando o cliente respondia, nada casava o
        // gatilho "Avancar" e a conversa era finalizada sem nunca perguntar nada.
        //
        // Para nas etapas que esperam resposta (Capturar Input) e tem limite de saltos, senao um
        // encadeamento circular mal configurado deixaria o cliente recebendo mensagem sem parar.
        private async Task<FlowEtapa> EncadearEtapasSemRespostaAsync(
            FlowEtapa etapaExecutada, ConversationState estadoAtual, Guid empresaId, string celular, string? phoneNumberIdOrigem)
        {
            const int limiteDeSaltos = 10;
            var atual = etapaExecutada;

            for (var salto = 0; salto < limiteDeSaltos; salto++)
            {
                if (atual.NomeEtapa == "Capturar Input") break;

                var proxima = await _unitOfWork.Flow.ObterProximaEtapa(atual.Id, "Avancar");
                if (proxima == null) break;

                await ExecutarEtapa(proxima, estadoAtual.Variaveis, estadoAtual, empresaId, celular, phoneNumberIdOrigem);
                atual = proxima;

                if (salto == limiteDeSaltos - 1)
                {
                    _logger.LogWarning(
                        "Flow {FlowId} encadeou {Limite} etapas sem esperar resposta; parando por seguranca.",
                        estadoAtual.FlowId, limiteDeSaltos);
                }
            }

            return atual;
        }

        // "prefixo" so e usado ao REPERGUNTAR uma etapa que o cliente respondeu fora do
        // esperado: entra antes do texto original ("Nao entendi... " + a pergunta de novo).
        private async Task ExecutarEtapa(FlowEtapa etapa, string? variaveisJson, ConversationState? estadoAtual, Guid empresaId, string celular, string? phoneNumberIdOrigem = null, string? prefixo = null)
        {
            // "Capturar Input" tambem envia o texto dela: a tela de Flows pede a pergunta nessa
            // etapa ("Qual seu nome?") e ela simplesmente nunca era enviada -- so etapa do tipo
            // "Mensagem" falava com o cliente. Na pratica, um flow que fazia pergunta na etapa de
            // captura ficava mudo esperando resposta de uma pergunta que ninguem tinha feito.
            var enviaTexto = etapa.NomeEtapa == "Mensagem" || etapa.NomeEtapa == "Capturar Input";

            if (enviaTexto && !string.IsNullOrEmpty(etapa.ConteudoLivre))
            {
                // Substitui variaveis no formato {{nome}} pelos valores capturados
                var mensagem = etapa.ConteudoLivre;

                if (!string.IsNullOrEmpty(variaveisJson))
                {
                    var variaveis = JsonConvert.DeserializeObject<Dictionary<string, string>>(variaveisJson);
                    if (variaveis != null)
                    {
                        foreach (var kvp in variaveis)
                        {
                            mensagem = mensagem.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                        }
                    }
                }

                // Variavel que nunca foi preenchida nao pode chegar crua no cliente ("Oi,
                // {{nome}}!"). Acontece de verdade quando a ramificacao pula a etapa que
                // capturava aquele dado. Some com o marcador e limpa o espaco/virgula que
                // sobra antes dele, pra frase continuar lendo bem.
                mensagem = Regex.Replace(mensagem, @",?\s*\{\{\w+\}\}", string.Empty);

                if (!string.IsNullOrEmpty(prefixo)) mensagem = prefixo + mensagem;

                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(empresaId);
                // Responde pelo mesmo numero que recebeu a mensagem (phoneNumberIdOrigem) --
                // sem isso, toda resposta de Flow saia pelo Empresa.PhoneNumberId "padrao" da
                // empresa, mesmo quando o cliente escreveu pra outro numero conectado.
                var phoneNumberId = phoneNumberIdOrigem ?? await _unitOfWork.Empresa.ObterPhoneNumberId(empresaId);

                // Capturar Input com Botao1/Botao2 preenchidos manda botoes de resposta rapida
                // em vez de texto simples -- o cliente escolhe tocando, nao digitando.
                var wamid = !string.IsNullOrEmpty(etapa.Botao1) && !string.IsNullOrEmpty(etapa.Botao2)
                    ? await _metaService.EnviarBotoesAsync(celular, mensagem, new List<string> { etapa.Botao1, etapa.Botao2 }, token, phoneNumberId)
                    : await _metaService.EnviarTextoLivreAsync(celular, mensagem, token, phoneNumberId);

                await _unitOfWork.HistoricoDisparo.Incluir(new HistoricoDisparo
                {
                    EmpresaId = empresaId,
                    ContatoId = estadoAtual?.ContatoId ?? Guid.Empty,
                    TipoDisparo = "Flow",
                    Conteudo = mensagem,
                    WamidMeta = wamid,
                    DataEnvio = DateTime.Now
                });

                // Avisa o painel na hora: sem isto, quem estava olhando o Chats via a mensagem do
                // cliente aparecer e a resposta do bot so depois de recarregar a pagina.
                await _notificadorChat.NotificarMensagemEnviadaAsync(
                    empresaId, estadoAtual?.ContatoId ?? Guid.Empty, mensagem, wamid);
            }
            else if (etapa.TemplateId.HasValue)
            {
                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(etapa.TemplateId.Value, empresaId);
                if (template == null)
                {
                    _logger.LogWarning(
                        "Flow: etapa {EtapaId} referencia Template {TemplateId} que nao foi encontrado (ou nao pertence a empresa {EmpresaId}). Envio ignorado.",
                        etapa.Id, etapa.TemplateId, empresaId);
                    return;
                }

                // Os parametros posicionais do HSM sao resolvidos na mesma convencao {{variavel}}
                // usada nas etapas de Mensagem acima, mas aqui em vez de substituir no texto,
                // cada ocorrencia vira um item da lista de parametros exigida pela API de envio
                // de Template da Meta (ordem de aparicao no corpo do template).
                var variaveis = string.IsNullOrEmpty(variaveisJson)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(variaveisJson) ?? new Dictionary<string, string>();

                var parametrosBody = Regex.Matches(template.Conteudo ?? string.Empty, @"\{\{(\w+)\}\}")
                    .Select(m => variaveis.TryGetValue(m.Groups[1].Value, out var valor) ? valor : string.Empty)
                    .ToList();

                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(empresaId);
                // Mesmo motivo do envio de texto livre acima: responde pelo numero de origem.
                var phoneNumberId = phoneNumberIdOrigem ?? await _unitOfWork.Empresa.ObterPhoneNumberId(empresaId);

                var command = new EnviarMensagemTemplateMetaCommand
                {
                    IdEmpresa = empresaId,
                    EmpresaId = empresaId,
                    ContatoId = estadoAtual?.ContatoId ?? Guid.Empty,
                    Telefone = celular,
                    TemplateId = template.Id,
                    NomeTemplate = template.NomeTemplate,
                    Idioma = template.Idioma,
                    ParametrosBody = parametrosBody
                };

                var resultadoEnvio = await _metaService.EnviarTemplateAsync(command, phoneNumberId, token);

                if (resultadoEnvio == null || !resultadoEnvio.Sucesso)
                {
                    _logger.LogWarning(
                        "Flow: falha ao enviar Template {TemplateId} na etapa {EtapaId}: {Erro}",
                        etapa.TemplateId, etapa.Id, resultadoEnvio?.Erro ?? "resposta nula da Meta");
                    return;
                }

                await _unitOfWork.HistoricoDisparo.Incluir(new HistoricoDisparo
                {
                    EmpresaId = empresaId,
                    ContatoId = estadoAtual?.ContatoId ?? Guid.Empty,
                    TemplateId = template.Id,
                    TipoDisparo = "Flow",
                    Conteudo = TemplateTextoHelper.MontarTextoEnviado(template.Conteudo, template.NomeTemplate, parametrosBody),
                    WamidMeta = resultadoEnvio.WamidMeta,
                    DataEnvio = DateTime.Now
                });
            }
        }

        private static string? ObterVariavelSaida(FlowEtapa etapa)
        {
            // O que a tela configurou manda.
            if (!string.IsNullOrWhiteSpace(etapa.VariavelSaida))
                return etapa.VariavelSaida.Trim();

            // Fallback pros flows criados antes da coluna existir: a variavel era adivinhada
            // procurando {{algo}} dentro da propria pergunta. Nao vale como comportamento
            // desejado -- o cliente veria "{{nome}}" cru na mensagem -- mas mantem funcionando
            // quem por acaso escreveu assim.
            if (string.IsNullOrEmpty(etapa.ConteudoLivre))
                return null;

            var match = Regex.Match(etapa.ConteudoLivre, @"\{\{(\w+)\}\}");
            return match.Success ? match.Groups[1].Value : null;
        }

        // "Pro" casa antes de "Starter" ser o default -- se a resposta nao bater com nenhum dos
        // dois (cliente digitou algo em vez de tocar o botao), manda o link do Starter por ser
        // o plano de entrada, evitando travar o flow sem link nenhum.
        private string ResolverLinkPagamento(string respostaCliente)
        {
            var resposta = (respostaCliente ?? string.Empty).Trim();

            if (resposta.Contains("pro", StringComparison.OrdinalIgnoreCase))
                return _planosConfig.LinkPro;

            return _planosConfig.LinkStarter;
        }
    }
}
