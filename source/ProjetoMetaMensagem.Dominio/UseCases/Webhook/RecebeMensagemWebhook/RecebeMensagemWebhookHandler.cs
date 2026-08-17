using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers.HTMLHelper;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook
{
    public class RecebeMensagemWebhookHandler : IRequestHandler<RecebeMensagemWebhookCommand, Response<RecebeMensagemWebhookResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFlowOrchestratorService _flowOrchestratorService;
        private readonly IWebhookDispatcherService _webhookDispatcherService;
        private readonly ILogger<RecebeMensagemWebhookHandler> _logger;

        public RecebeMensagemWebhookHandler(IUnitOfWork unitOfWork, IFlowOrchestratorService flowOrchestratorService, IWebhookDispatcherService webhookDispatcherService, ILogger<RecebeMensagemWebhookHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _flowOrchestratorService = flowOrchestratorService;
            _webhookDispatcherService = webhookDispatcherService;
            _logger = logger;
        }

        public async Task<Response<RecebeMensagemWebhookResult>> Handle(RecebeMensagemWebhookCommand command)
        {
            var response = new Response<RecebeMensagemWebhookResult>();

            try
            {
                var validator = new RecebeMensagemWebhookValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                if (command.Entry == null || !command.Entry.Any()) return response;

                var mensagensParaSalvar = new List<Entidades.MensagemRecebida>();
                var statusAtualizados = new List<StatusAtualizadoBroadcastDto>();

                foreach (var entry in command.Entry)
                {
                    if (entry.Changes == null) continue;

                    foreach (var change in entry.Changes)
                    {
                        var metadata = change.Value?.Metadata;
                        if (string.IsNullOrEmpty(metadata?.PhoneNumberId)) continue;

                        // Numero especifico que recebeu o evento (empresas podem ter mais de um
                        // numero conectado). Usado tanto pra achar a empresa quanto pra responder
                        // pelo mesmo numero no Flow.
                        var numeroOrigem = await _unitOfWork.Numero.ObterPorInstanciaId(metadata.PhoneNumberId);

                        // Resolve a empresa preferencialmente pelo Numero (funciona pra qualquer
                        // numero da empresa, nao so o "principal"). Cai pro Empresa.PhoneNumberId
                        // so quando a empresa nunca cadastrou um Numero com esse InstanciaId --
                        // sem esse fallback, empresas antigas que so tem PhoneNumberId direto na
                        // Empresa (sem linha em Numero) parariam de receber mensagens.
                        Guid? empresaId = null;
                        if (numeroOrigem != null)
                        {
                            var usuarioDoNumero = await _unitOfWork.Usuario.ObterPorId(numeroOrigem.UsuarioId);
                            empresaId = usuarioDoNumero?.EmpresaId;
                        }
                        empresaId ??= await _unitOfWork.Empresa.ObterPorPhoneNumberId(metadata.PhoneNumberId);

                        // Se não encontrar uma empresa cadastrada para esse PhoneNumberId, pula o processamento
                        if (!empresaId.HasValue) continue;

                        // Processa os statuses de entrega (sent/delivered/read/failed) do disparo
                        if (change.Value?.Statuses != null && change.Value.Statuses.Any())
                        {
                            foreach (var statusMeta in change.Value.Statuses)
                            {
                                if (string.IsNullOrEmpty(statusMeta.Id) || string.IsNullOrEmpty(statusMeta.Status)) continue;

                                string? erroDetalhado = null;
                                if (statusMeta.Status == "failed" && statusMeta.Errors != null && statusMeta.Errors.Any())
                                {
                                    var primeiroErro = statusMeta.Errors.First();
                                    erroDetalhado = $"({primeiroErro.Code}) {primeiroErro.Title}: {primeiroErro.ErrorData?.Details ?? primeiroErro.Message}";
                                    _logger.LogWarning("Falha na entrega da mensagem {Wamid}: {Erro}", statusMeta.Id, erroDetalhado);
                                }

                                await _unitOfWork.HistoricoDisparo.AtualizarStatusEntregaPorWamid(statusMeta.Id, statusMeta.Status, erroDetalhado);

                                var statusDto = new StatusAtualizadoBroadcastDto
                                {
                                    WamidMeta = statusMeta.Id,
                                    Status = statusMeta.Status,
                                    EmpresaId = empresaId.Value,
                                    Erro = erroDetalhado
                                };
                                statusAtualizados.Add(statusDto);

                                // Dispara pros webhooks configurados pelo tenant, isolado em
                                // try/catch proprio pelo mesmo motivo do Flow logo abaixo: um
                                // webhook de terceiro fora do ar nao pode derrubar o processamento
                                // do evento da Meta.
                                try
                                {
                                    await _webhookDispatcherService.Disparar("status_atualizado", statusDto, empresaId.Value);
                                }
                                catch (Exception exWebhook)
                                {
                                    _logger.LogError(exWebhook, "Falha ao disparar webhooks configurados para status_atualizado (wamid {Wamid})", statusMeta.Id);
                                }
                            }
                        }

                        if (change.Value?.Messages == null || !change.Value.Messages.Any()) continue;

                        // Numero da propria empresa que recebeu o evento (digitos apenas), usado
                        // abaixo pra detectar mensagens "ecoadas" pelo WhatsApp Business App em
                        // clientes com Coexistencia ativa -- sem isso, uma resposta que o atendente
                        // manda pelo celular chega aqui pelo webhook igual a uma mensagem do cliente
                        // e aparece do lado errado no chat (como se o cliente tivesse mandado).
                        var numeroDaEmpresa = new string((metadata.DisplayPhoneNumber ?? "").Where(char.IsDigit).ToArray());

                        foreach (var msgMeta in change.Value.Messages)
                        {
                            if (string.IsNullOrEmpty(msgMeta.From)) continue;

                            var remetenteNormalizado = new string(msgMeta.From.Where(char.IsDigit).ToArray());
                            if (!string.IsNullOrEmpty(numeroDaEmpresa) && remetenteNormalizado == numeroDaEmpresa)
                            {
                                // Mensagem enviada pelo proprio numero da empresa (ex: atendente
                                // respondeu direto pelo app) -- nao e uma mensagem recebida do
                                // cliente, entao nao processa aqui. TODO: capturar isso no
                                // historico de disparo pra aparecer do lado certo no chat.
                                continue;
                            }

                            // Busca o contato se existir, mas não bloqueia caso não exista
                            var contato = await _unitOfWork.Contato.ObterPorTelefone(empresaId.Value, msgMeta.From);

                            // Quem manda mensagem e nao esta na base vira contato aqui.
                            //
                            // Sem isto, dois furos: o flow so rodava pra contato conhecido -- e
                            // lead de anuncio e, por definicao, desconhecido, entao ele escrevia e
                            // NAO recebia resposta nenhuma -- e a conversa aparecia no chat sem
                            // nome, so com o telefone. O nome vem do perfil do WhatsApp.
                            if (contato == null)
                            {
                                contato = await CriarContatoDoRemetenteAsync(
                                    empresaId.Value,
                                    msgMeta.From,
                                    change.Value.Contacts?.FirstOrDefault()?.Profile?.Name);
                            }

                            var novaMensagem = new Entidades.MensagemRecebida
                            {
                                Id = Guid.NewGuid(),
                                EmpresaId = empresaId.Value,
                                TelefoneRemetente = msgMeta.From,
                                Tipo = "recebida",
                                Lida = false,
                                ContatoId = contato?.Id, // Mantém null se o contato não existir no banco
                                FlowId = null,
                                // Todo o resto do sistema grava DateTime.Now (hora local). Gravar
                                // UTC so aqui fazia a mensagem recebida aparecer 3h no futuro no
                                // chat e no relatorio, bagunçar a ordenação da lista unificada e
                                // quebrar o "tempo relativo", que calculava diferença negativa.
                                DataRecebimento = DateTime.Now
                            };

                            // Define o conteúdo conforme o tipo de mensagem
                            if (msgMeta.Type == "text" && msgMeta.Text != null)
                            {
                                novaMensagem.Conteudo = msgMeta.Text.Body ?? string.Empty;
                            }
                            else if (msgMeta.Type == "image" && msgMeta.Image != null)
                            {
                                novaMensagem.MidiaId = msgMeta.Image.Id;
                                novaMensagem.TipoMidia = "image";
                                novaMensagem.Conteudo = "[Imagem]";
                            }
                            else if (msgMeta.Type == "audio" && msgMeta.Audio != null)
                            {
                                novaMensagem.MidiaId = msgMeta.Audio.Id;
                                novaMensagem.TipoMidia = "audio";
                                novaMensagem.Conteudo = "[Áudio]";
                            }
                            else if (msgMeta.Type == "document" && msgMeta.Document != null)
                            {
                                novaMensagem.MidiaId = msgMeta.Document.Id;
                                novaMensagem.TipoMidia = "document";
                                novaMensagem.Conteudo = "[Documento]";
                            }
                            else if (msgMeta.Type == "button" && msgMeta.Button != null)
                            {
                                // O cliente tocou num botao do template: pro sistema isso e
                                // texto normal, e e assim que o flow reconhece a resposta dele.
                                novaMensagem.Conteudo = msgMeta.Button.Text ?? msgMeta.Button.Payload ?? string.Empty;
                            }
                            else if (msgMeta.Type == "interactive" && msgMeta.Interactive != null)
                            {
                                novaMensagem.Conteudo =
                                    msgMeta.Interactive.ButtonReply?.Title
                                    ?? msgMeta.Interactive.ListReply?.Title
                                    ?? string.Empty;
                            }
                            else
                            {
                                novaMensagem.Conteudo = $"[Mídia do tipo {msgMeta.Type}]";
                            }

                            mensagensParaSalvar.Add(novaMensagem);

                            // Chegou por anuncio Click-to-WhatsApp: guarda de qual anuncio veio.
                            // A Meta so manda o referral na PRIMEIRA mensagem da pessoa -- se
                            // passar batido aqui, nao ha como creditar a venda ao anuncio depois.
                            if (msgMeta.Referral != null && !string.IsNullOrWhiteSpace(msgMeta.From))
                            {
                                try
                                {
                                    await _unitOfWork.OrigemLead.Incluir(new Entidades.OrigemLead
                                    {
                                        EmpresaId = empresaId.Value,
                                        ContatoId = contato?.Id,
                                        Telefone = msgMeta.From!,
                                        CtwaClid = msgMeta.Referral.CtwaClid,
                                        SourceId = msgMeta.Referral.SourceId,
                                        SourceType = msgMeta.Referral.SourceType,
                                        SourceUrl = msgMeta.Referral.SourceUrl,
                                        Headline = msgMeta.Referral.Headline,
                                        Corpo = msgMeta.Referral.Body
                                    });

                                    _logger.LogInformation(
                                        "Lead veio do anuncio {SourceId} (tipo {SourceType})",
                                        msgMeta.Referral.SourceId, msgMeta.Referral.SourceType);
                                }
                                catch (Exception exOrigem)
                                {
                                    // Mesmo isolamento do resto: perder a origem e ruim, perder a
                                    // mensagem do cliente e pior.
                                    _logger.LogError(exOrigem, "Falha ao gravar a origem do lead {Telefone}", msgMeta.From);
                                }
                            }

                            // Dispara pros webhooks configurados pelo tenant. Mesmo isolamento em
                            // try/catch do bloco de status acima -- nao pode derrubar o salvamento
                            // da mensagem, que e o efeito principal deste webhook.
                            try
                            {
                                await _webhookDispatcherService.Disparar("mensagem_recebida", new MensagemRecebidaBroadcastDto
                                {
                                    EmpresaId = novaMensagem.EmpresaId,
                                    ContatoId = novaMensagem.ContatoId,
                                    TelefoneRemetente = novaMensagem.TelefoneRemetente,
                                    Conteudo = novaMensagem.Conteudo,
                                    DataRecebimento = novaMensagem.DataRecebimento,
                                    MidiaId = novaMensagem.MidiaId,
                                    TipoMidia = novaMensagem.TipoMidia
                                }, empresaId.Value);
                            }
                            catch (Exception exWebhook)
                            {
                                _logger.LogError(exWebhook, "Falha ao disparar webhooks configurados para mensagem_recebida de {Telefone}", msgMeta.From);
                            }

                            // Dispara o Flow so pra contato ja cadastrado (sem isso nao ha
                            // ConversationState.ContatoId pra gravar) e so pra texto livre --
                            // e o unico tipo de gatilho/resposta que o orquestrador hoje casa.
                            // Isolado num try/catch proprio: uma falha no Flow (banco, Meta,
                            // JSON de variaveis malformado) nao pode derrubar o salvamento da
                            // mensagem recebida, que e o efeito principal deste webhook.
                            // Inclui resposta de botao e de lista: pro flow, o texto do botao que
                            // o cliente tocou vale como resposta dele igual a texto digitado.
                            var tipoQueAlimentaFlow = msgMeta.Type == "text" || msgMeta.Type == "button" || msgMeta.Type == "interactive";

                            if (contato != null && tipoQueAlimentaFlow && !string.IsNullOrWhiteSpace(novaMensagem.Conteudo))
                            {
                                try
                                {
                                    await _flowOrchestratorService.ProcessarMensagem(empresaId.Value, contato.Id, msgMeta.From, novaMensagem.Conteudo, metadata.PhoneNumberId, numeroOrigem?.Id);
                                }
                                catch (Exception exFlow)
                                {
                                    _logger.LogError(exFlow, "Falha ao processar Flow para mensagem recebida de {Telefone}", msgMeta.From);
                                }
                            }
                        }
                    }
                }

                // Salva todas as mensagens mapeadas
                if (mensagensParaSalvar.Any())
                {
                    foreach (var mensagem in mensagensParaSalvar)
                    {
                        await _unitOfWork.MensagemRecebida.Incluir(mensagem);
                    }
                }

                response.AddValue(new RecebeMensagemWebhookResult
                {
                    Sucesso = true,
                    Mensagem = mensagensParaSalvar.FirstOrDefault()?.Conteudo ?? "Evento processado sem novas mensagens.",
                    MensagensSalvas = mensagensParaSalvar.Select(m => new MensagemRecebidaBroadcastDto
                    {
                        EmpresaId = m.EmpresaId,
                        ContatoId = m.ContatoId,
                        TelefoneRemetente = m.TelefoneRemetente,
                        Conteudo = m.Conteudo,
                        DataRecebimento = m.DataRecebimento,
                        MidiaId = m.MidiaId,
                        TipoMidia = m.TipoMidia
                    }).ToList(),
                    StatusAtualizados = statusAtualizados
                });
            }
            catch (Exception ex) 
            {
                response.AddErroServico(ex, _logger, nameof(RecebeMensagemWebhookHandler));
            }

            return response;
        }

        // O Contato pertence a um usuario, nao a empresa direto: usa o primeiro admin dela, que e
        // quem "recebe" o lead. Falha aqui nao derruba o webhook -- a mensagem ja foi salva.
        private async Task<Entidades.Contato?> CriarContatoDoRemetenteAsync(Guid empresaId, string telefone, string? nomePerfil)
        {
            try
            {
                var usuarios = await _unitOfWork.Usuario.ObterPorEmpresa(empresaId);
                var dono = usuarios.FirstOrDefault(u => u.IsAdmin == true) ?? usuarios.FirstOrDefault();

                if (dono == null)
                {
                    _logger.LogWarning("Empresa {EmpresaId} sem usuario: contato de {Telefone} nao criado", empresaId, telefone);
                    return null;
                }

                var novoContato = new Entidades.Contato
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = dono.Id,
                    Telefone = telefone,
                    Nome = string.IsNullOrWhiteSpace(nomePerfil) ? telefone : nomePerfil,
                    DataCriacao = DateTime.Now
                };

                await _unitOfWork.Contato.Incluir(novoContato);
                _logger.LogInformation("Contato criado automaticamente para {Telefone} ({Nome})", telefone, novoContato.Nome);

                return novoContato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar contato automatico para {Telefone}", telefone);
                return null;
            }
        }
    }
}
