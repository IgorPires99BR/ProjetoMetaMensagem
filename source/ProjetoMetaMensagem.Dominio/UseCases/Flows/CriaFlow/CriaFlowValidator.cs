using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow
{
    public class CriaFlowValidator : AbstractValidator<CriaFlowCommand>
    {
        public CriaFlowValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do fluxo.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do fluxo.")
                .MaximumLength(150).WithMessage("O nome do fluxo deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Etapas)
                .NotEmpty().WithMessage("O fluxo precisa de ao menos uma etapa.");
        }
    }
}
