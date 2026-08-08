using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato
{
    public class AssociarTagsContatoValidator : AbstractValidator<AssociarTagsContatoCommand>
    {
        public AssociarTagsContatoValidator()
        {
            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato.");

            RuleFor(x => x.TagIds)
                .NotNull().WithMessage("Informe as tags a associar.");
        }
    }
}
