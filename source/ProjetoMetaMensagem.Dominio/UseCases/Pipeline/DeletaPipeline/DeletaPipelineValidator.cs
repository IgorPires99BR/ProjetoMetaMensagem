using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.DeletaPipeline
{
    public class DeletaPipelineValidator : AbstractValidator<DeletaPipelineCommand>
    {
        public DeletaPipelineValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o pipeline que será excluído.");
        }
    }
}
