using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup
{
    public class IniciaEmbeddedSignupValidator : AbstractValidator<IniciaEmbeddedSignupCommand>
    {
        public IniciaEmbeddedSignupValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("O 'code' retornado pelo Embedded Signup é obrigatório.");
            RuleFor(x => x.IdEmpresa).NotEmpty().WithMessage("A empresa é obrigatória.");
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O usuário é obrigatório.");
            RuleFor(x => x.NumeroTelefone).NotEmpty().WithMessage("O telefone é obrigatório.");
        }
    }
}
