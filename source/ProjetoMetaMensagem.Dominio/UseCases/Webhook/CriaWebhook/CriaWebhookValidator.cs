using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook
{
    public class CriaWebhookValidator : AbstractValidator<CriaWebhookCommand>
    {
        public CriaWebhookValidator()
        {
            RuleFor(c => c.EmpresaId)
                .NotEmpty().WithMessage("EmpresaId é obrigatório.");

            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.");

            RuleFor(c => c.Url)
                .NotEmpty().WithMessage("Url é obrigatória.");

            RuleFor(c => c.Evento)
                .NotEmpty().WithMessage("Evento é obrigatório.");
        }
    }
}
