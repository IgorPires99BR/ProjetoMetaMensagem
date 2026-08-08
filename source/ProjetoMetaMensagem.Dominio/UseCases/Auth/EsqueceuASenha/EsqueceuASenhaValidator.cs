using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha
{
    public class EsqueceuASenhaValidator : AbstractValidator<EsqueceuASenhaCommand>
    {
        public EsqueceuASenhaValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail.")
                .EmailAddress().WithMessage("Informe um e-mail válido.");
        }
    }
}
