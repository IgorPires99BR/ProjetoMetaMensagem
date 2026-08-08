using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow
{
    public class DeletaFlowValidator : AbstractValidator<DeletaFlowCommand>
    {
        public DeletaFlowValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o fluxo que será excluído.");
        }
    }
}
