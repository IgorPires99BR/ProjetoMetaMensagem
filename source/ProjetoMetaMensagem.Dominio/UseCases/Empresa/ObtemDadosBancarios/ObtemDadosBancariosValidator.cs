using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosValidator : AbstractValidator<ObtemDadosBancariosCommand>
    {
        public ObtemDadosBancariosValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
