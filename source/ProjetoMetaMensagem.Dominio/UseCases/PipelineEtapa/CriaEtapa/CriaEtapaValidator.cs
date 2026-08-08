using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa
{
    public class CriaEtapaValidator : AbstractValidator<CriaEtapaCommand>
    {
        public CriaEtapaValidator()
        {
            RuleFor(x => x.PipelineId)
                .NotEmpty().WithMessage("Informe o pipeline dono da etapa.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome da etapa.")
                .MaximumLength(100).WithMessage("O nome da etapa deve ter no máximo 100 caracteres.");
        }
    }
}
