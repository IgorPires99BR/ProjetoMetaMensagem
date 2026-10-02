using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.SolicitaNomeNumero
{
    public class SolicitaNomeNumeroValidator : AbstractValidator<SolicitaNomeNumeroCommand>
    {
        public SolicitaNomeNumeroValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");
            RuleFor(x => x.NovoNome)
                .NotEmpty().WithMessage("Informe o novo nome.")
                .MaximumLength(100).WithMessage("O nome pode ter no máximo 100 caracteres.");
        }
    }
}
