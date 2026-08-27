using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.TrocaSenha
{
    public class TrocaSenhaValidator : AbstractValidator<TrocaSenhaCommand>
    {
        public TrocaSenhaValidator()
        {
            RuleFor(x => x.SenhaAtual)
                .NotEmpty().WithMessage("Informe a senha atual.");

            RuleFor(x => x.SenhaNova)
                .NotEmpty().WithMessage("Informe a nova senha.")
                .MinimumLength(6).WithMessage("A nova senha deve ter ao menos 6 caracteres.")
                .MaximumLength(72).WithMessage("A nova senha deve ter no máximo 72 caracteres.");

            RuleFor(x => x.ConfirmacaoSenhaNova)
                .Equal(x => x.SenhaNova).WithMessage("A confirmação não confere com a nova senha.");

            // Trocar a senha pela mesma senha da a impressao de ter funcionado sem mudar nada --
            // e quem chega aqui vindo do e-mail de boas-vindas esta justamente tentando sair da
            // senha sorteada.
            RuleFor(x => x.SenhaNova)
                .NotEqual(x => x.SenhaAtual).WithMessage("A nova senha precisa ser diferente da atual.")
                .When(x => !string.IsNullOrEmpty(x.SenhaAtual));
        }
    }
}
