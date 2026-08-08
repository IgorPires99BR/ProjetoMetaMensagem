using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario
{
    public class CriaUsuarioValidator : AbstractValidator<CriaUsuarioCommand>
    {
        public CriaUsuarioValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do usuário.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do usuário.")
                .MaximumLength(255).WithMessage("O nome do usuário deve ter no máximo 255 caracteres.");

            // O e-mail e a chave usada pelo login e pela recuperacao de senha, entao nao pode faltar.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail do usuário.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.");

            // Chega em texto puro no command e so vira hash BCrypt dentro do handler.
            RuleFor(x => x.SenhaHash)
                .NotEmpty().WithMessage("Informe a senha do usuário.")
                .MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.");
        }
    }
}
