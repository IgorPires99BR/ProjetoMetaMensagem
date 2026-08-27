using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente
{
    public class CriaContaClienteValidator : AbstractValidator<CriaContaClienteCommand>
    {
        public CriaContaClienteValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do cliente.")
                .MaximumLength(255).WithMessage("O nome deve ter no máximo 255 caracteres.");

            // O e-mail e o login do cliente e o endereco por onde a senha chega: sem ele a
            // conta nasce sem ninguem conseguir entrar nela.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail do cliente.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Plano)
                .NotEmpty().WithMessage("Escolha o plano do cliente.");
        }
    }
}
