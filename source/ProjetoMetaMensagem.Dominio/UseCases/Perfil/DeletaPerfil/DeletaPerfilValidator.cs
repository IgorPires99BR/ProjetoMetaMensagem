using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.DeletaPerfil
{
    public class DeletaPerfilValidator : AbstractValidator<DeletaPerfilCommand>
    {
        public DeletaPerfilValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o perfil que será excluído.");
        }
    }
}
