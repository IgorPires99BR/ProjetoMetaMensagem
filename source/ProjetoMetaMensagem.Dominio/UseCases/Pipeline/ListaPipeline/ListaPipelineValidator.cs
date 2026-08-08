using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline
{
    public class ListaPipelineValidator : AbstractValidator<ListaPipelineCommand>
    {
        public ListaPipelineValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
