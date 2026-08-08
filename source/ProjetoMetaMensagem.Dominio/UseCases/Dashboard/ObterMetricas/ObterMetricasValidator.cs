using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas
{
    public class ObterMetricasValidator : AbstractValidator<ObterMetricasCommand>
    {
        public ObterMetricasValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
