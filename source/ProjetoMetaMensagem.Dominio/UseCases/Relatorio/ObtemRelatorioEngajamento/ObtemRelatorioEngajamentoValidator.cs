using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento
{
    public class ObtemRelatorioEngajamentoValidator : AbstractValidator<ObtemRelatorioEngajamentoCommand>
    {
        public ObtemRelatorioEngajamentoValidator()
        {
            RuleFor(x => x.DataFim)
                .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data final não pode ser anterior à data inicial.")
                .When(x => x.DataInicio.HasValue && x.DataFim.HasValue);
        }
    }
}
