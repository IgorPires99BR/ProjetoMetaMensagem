using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto
{
    public class ProcessaEventoCaktoValidator : AbstractValidator<ProcessaEventoCaktoCommand>
    {
        public ProcessaEventoCaktoValidator()
        {
            RuleFor(x => x.Evento)
                .NotEmpty().WithMessage("Evento sem nome.");

            RuleFor(x => x.Dados)
                .NotNull().WithMessage("Evento sem dados.");

            // O id do evento é a chave de idempotência: sem ele não dá para saber se a Cakto está
            // reenviando algo já processado.
            RuleFor(x => x.Dados!.Id)
                .NotEmpty().WithMessage("Evento sem identificador.")
                .When(x => x.Dados != null);
        }
    }
}
