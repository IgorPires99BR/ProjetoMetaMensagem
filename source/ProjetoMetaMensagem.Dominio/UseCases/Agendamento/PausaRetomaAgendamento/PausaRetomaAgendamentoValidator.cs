using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.PausaRetomaAgendamento
{
    public class PausaRetomaAgendamentoValidator : AbstractValidator<PausaRetomaAgendamentoCommand>
    {
        public PausaRetomaAgendamentoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Agendamento não identificado.");
        }
    }
}
