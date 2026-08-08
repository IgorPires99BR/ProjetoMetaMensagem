using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario
{
    public class DeletaUsuarioValidator : AbstractValidator<DeletaUsuarioCommand>
    {
        public DeletaUsuarioValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o usuário que será excluído.");
        }
    }
}
