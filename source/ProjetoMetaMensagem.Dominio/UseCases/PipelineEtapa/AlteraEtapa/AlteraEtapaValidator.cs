using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.AlteraEtapa
{
    public class AlteraEtapaValidator : AbstractValidator<AlteraEtapaCommand>
    {
        public AlteraEtapaValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe a etapa que será alterada.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome da etapa.")
                .MaximumLength(100).WithMessage("O nome da etapa deve ter no máximo 100 caracteres.");
        }
    }
}
