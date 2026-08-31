using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemFunilFlow
{
    public class ObtemFunilFlowValidator : AbstractValidator<ObtemFunilFlowCommand>
    {
        public ObtemFunilFlowValidator()
        {
            RuleFor(x => x.FlowId)
                .NotEmpty().WithMessage("Informe o flow para calcular o funil.");
        }
    }
}
