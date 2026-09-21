using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.CriaPerfil
{
    public class CriaPerfilValidator : AbstractValidator<CriaPerfilCommand>
    {
        public CriaPerfilValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do perfil.")
                .MaximumLength(100).WithMessage("O nome do perfil deve ter no máximo 100 caracteres.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do perfil.");
        }
    }
}
