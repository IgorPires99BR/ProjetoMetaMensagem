using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers.HTMLHelper;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook;
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

        public RecebeMensagemWebhookHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                        // Busca a empresa associada ao PhoneNumberId recebido
                        Guid? empresaId = await _unitOfWork.Empresa.ObterPorPhoneNumberId(metadata.PhoneNumberId);

                        // Se não encontrar uma empresa cadastrada para esse PhoneNumberId, pula o processamento
                        if (!empresaId.HasValue) continue;

                        // Processa os statuses de entrega (sent/delivered/read/failed) do disparo
                        if (change.Value?.Statuses != null && change.Value.Statuses.Any())
                        {
                            foreach (var statusMeta in change.Value.Statuses)
                            {
                                if (string.IsNullOrEmpty(statusMeta.Id) || string.IsNullOrEmpty(statusMeta.Status)) continue;

                                await _unitOfWork.HistoricoDisparo.AtualizarStatusEntregaPorWamid(statusMeta.Id, statusMeta.Status);

                                statusAtualizados.Add(new StatusAtualizadoBroadcastDto
                                {
                                    WamidMeta = statusMeta.Id,
                                    Status = statusMeta.Status,
                                    EmpresaId = empresaId.Value
                                });
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

                            var novaMensagem = new Entidades.MensagemRecebida
                            {
                                Id = Guid.NewGuid(),
                                EmpresaId = empresaId.Value,
                                TelefoneRemetente = msgMeta.From,
                                Tipo = "recebida",
                                Lida = false,
                                ContatoId = contato?.Id, // Mantém null se o contato não existir no banco
                                FlowId = null,
                                DataRecebimento = DateTime.UtcNow
                            };

                            // Define o conteúdo conforme o tipo de mensagem
                            if (msgMeta.Type == "text" && msgMeta.Text != null)
                            {
                                novaMensagem.Conteudo = msgMeta.Text.Body ?? string.Empty;
                            }
                            else
                            {
                                novaMensagem.Conteudo = $"[Mídia do tipo {msgMeta.Type}]";
                            }

                            mensagensParaSalvar.Add(novaMensagem);
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
                        DataRecebimento = m.DataRecebimento
                    }).ToList(),
                    StatusAtualizados = statusAtualizados
                });
            }
            catch (Exception ex) 
            {
                response.AddErro($"Erro ao processar e salvar mensagem recebida: {ex.Message}");
            }

            return response;
        }
    }
}
