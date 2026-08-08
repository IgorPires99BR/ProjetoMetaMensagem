using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Informe o e-mail.")
                .EmailAddress().WithMessage("Informe um e-mail válido.");

            RuleFor(x => x.password)
                .NotEmpty().WithMessage("Informe a senha.");
        }
    }
}
