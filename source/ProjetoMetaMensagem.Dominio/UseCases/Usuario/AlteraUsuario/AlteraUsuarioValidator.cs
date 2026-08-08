using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario
{
    public class AlteraUsuarioValidator : AbstractValidator<AlteraUsuarioCommand>
    {
        public AlteraUsuarioValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário que será alterado.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do usuário.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do usuário.")
                .MaximumLength(255).WithMessage("O nome do usuário deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail do usuário.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.");

            // Senha em branco na edicao significa "manter a senha atual" (o handler preserva o hash),
            // entao o tamanho minimo so vale quando o usuario realmente digitou uma senha nova.
            RuleFor(x => x.SenhaHash)
                .MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.SenhaHash));
        }
    }
}
