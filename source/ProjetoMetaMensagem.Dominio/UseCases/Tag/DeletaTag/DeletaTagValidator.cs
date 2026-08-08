using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag
{
    public class DeletaTagValidator : AbstractValidator<DeletaTagCommand>
    {
        public DeletaTagValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe a tag que será excluída.");
        }
    }
}
