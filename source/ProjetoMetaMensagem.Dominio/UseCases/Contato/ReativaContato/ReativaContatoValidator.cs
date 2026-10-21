using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ReativaContato
{
    public class ReativaContatoValidator : AbstractValidator<ReativaContatoCommand>
    {
        public ReativaContatoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o contato que será reativado.");
        }
    }
}
