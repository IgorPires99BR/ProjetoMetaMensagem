using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.AlteraPipeline
{
    public class AlteraPipelineValidator : AbstractValidator<AlteraPipelineCommand>
    {
        public AlteraPipelineValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o pipeline que será alterado.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do pipeline.")
                .MaximumLength(100).WithMessage("O nome do pipeline deve ter no máximo 100 caracteres.");
        }
    }
}
