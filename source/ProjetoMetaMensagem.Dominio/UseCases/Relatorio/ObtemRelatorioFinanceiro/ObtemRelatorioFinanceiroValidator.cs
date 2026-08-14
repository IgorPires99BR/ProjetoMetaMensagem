using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro
{
    public class ObtemRelatorioFinanceiroValidator : AbstractValidator<ObtemRelatorioFinanceiroCommand>
    {
        public ObtemRelatorioFinanceiroValidator()
        {
            RuleFor(x => x.DataFim)
                .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data final não pode ser anterior à data inicial.")
                .When(x => x.DataInicio.HasValue && x.DataFim.HasValue);
        }
    }
}
