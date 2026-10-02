using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemPerfilNumero
{
    public class ObtemPerfilNumeroValidator : AbstractValidator<ObtemPerfilNumeroCommand>
    {
        public ObtemPerfilNumeroValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");
        }
    }
}
