using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook
{
    // Sem regras de proposito: o payload e montado pela Meta, nao pelo usuario. A Meta envia
    // varios formatos de notificacao (mensagem, status, eventos de conta) e qualquer campo
    // exigido aqui faria o webhook rejeitar notificacoes legitimas e perder mensagens.
    // O handler ja ignora silenciosamente as entradas que nao sabe tratar.
    public class RecebeMensagemWebhookValidator : AbstractValidator<RecebeMensagemWebhookCommand>
    {
    }
}
