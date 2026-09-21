using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.AlteraPerfil
{
    public class AlteraPerfilValidator : AbstractValidator<AlteraPerfilCommand>
    {
        public AlteraPerfilValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o perfil que será alterado.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do perfil.")
                .MaximumLength(100).WithMessage("O nome do perfil deve ter no máximo 100 caracteres.");
        }
    }
}
