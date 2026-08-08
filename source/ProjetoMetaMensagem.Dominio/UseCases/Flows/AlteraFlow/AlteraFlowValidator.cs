using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow
{
    public class AlteraFlowValidator : AbstractValidator<AlteraFlowCommand>
    {
        public AlteraFlowValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o fluxo que será alterado.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do fluxo.")
                .MaximumLength(150).WithMessage("O nome do fluxo deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Etapas)
                .NotEmpty().WithMessage("O fluxo precisa de ao menos uma etapa.");
        }
    }
}
