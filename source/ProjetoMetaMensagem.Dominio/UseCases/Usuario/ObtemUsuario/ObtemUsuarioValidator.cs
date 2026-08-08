using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioValidator : AbstractValidator<ObtemUsuarioCommand>
    {
        public ObtemUsuarioValidator()
        {
            RuleFor(x => x.IdUsuario)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário consultado.");
        }
    }
}
