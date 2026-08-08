using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha
{
    public class CancelaCampanhaValidator : AbstractValidator<CancelaCampanhaCommand>
    {
        public CancelaCampanhaValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe a campanha que será cancelada.");
        }
    }
}
