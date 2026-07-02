using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
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

                if (command.Entry == null) return response;

                var mensagensParaSalvar = new List<MensagemRecebida>();

                // 1. Varre a árvore do payload da Meta
                foreach (var entry in command.Entry)
                {
                    if (entry.Changes == null) continue;

                    foreach (var change in entry.Changes)
                    {
                        if (change.Value?.Messages == null) continue;

                        var metadata = change.Value.Metadata;

                        // REGRA MULTI-TENANT CRÍTICA:
                        // Use o metadata.PhoneNumberId ou metadata.DisplayPhoneNumber para descobrir qual EmpresaId é dona desse número.
                        // Guid empresaId = await _empresaRepository.BuscarIdPorNumeroMetaAsync(metadata?.DisplayPhoneNumber);
                        Guid? empresaId = await _unitOfWork.Empresa.ObterPorPhoneNumberId(metadata.PhoneNumberId.ToString());// Substitua pela sua busca real

                        foreach (var msgMeta in change.Value.Messages)
                        {
                            // Cria o objeto utilizando a sua entidade existente
                            var novaMensagem = new MensagemRecebida
                            {
                                EmpresaId = empresaId ?? Guid.NewGuid(),
                                TelefoneRemetente = msgMeta.From,
                                Tipo = "recebida",
                                Lida = false,
                                ContatoId = null, // Caso queira buscar o ID do contato pelo telefoneRemetente no banco posteriormente
                                FlowId = null     // Se a mensagem fizer parte de um fluxo automatizado em execução
                            };

                            // Extrai o conteúdo baseado no tipo de mensagem que o cliente enviou
                            if (msgMeta.Type == "text" && msgMeta.Text != null)
                            {
                                novaMensagem.Conteudo = msgMeta.Text.Body;
                            }
                            else
                            {
                                novaMensagem.Conteudo = $"[Mídia do tipo {msgMeta.Type}]";
                            }

                            mensagensParaSalvar.Add(novaMensagem);
                        }
                    }
                }

                // 2. Salva no banco as mensagens mapeadas
                if (mensagensParaSalvar.Any())
                {
                    foreach (var mensagem in mensagensParaSalvar)
                    {
                        await _unitOfWork.MensagemRecebida.Incluir(mensagem);
                    }
                }

                response.AddValue(new RecebeMensagemWebhookResult { Sucesso = true ,Mensagem = mensagensParaSalvar.FirstOrDefault().Conteudo.ToString()});
            }
            catch (Exception ex) 
            {
                response.AddErro($"Erro ao processar e salvar mensagem recebida: {ex.Message}");
            }

            return response;
        }
    }
}
