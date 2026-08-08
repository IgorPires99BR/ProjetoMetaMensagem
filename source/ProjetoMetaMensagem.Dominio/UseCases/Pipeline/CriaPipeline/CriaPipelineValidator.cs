using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.CriaPipeline
{
    public class CriaPipelineValidator : AbstractValidator<CriaPipelineCommand>
    {
        public CriaPipelineValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do pipeline.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do pipeline.")
                .MaximumLength(100).WithMessage("O nome do pipeline deve ter no máximo 100 caracteres.");
        }
    }
}
