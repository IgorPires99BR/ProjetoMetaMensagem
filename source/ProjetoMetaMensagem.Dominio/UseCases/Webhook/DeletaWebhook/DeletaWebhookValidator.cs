using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook
{
    public class DeletaWebhookValidator : AbstractValidator<DeletaWebhookCommand>
    {
        public DeletaWebhookValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Id é obrigatório.");
        }
    }
}
