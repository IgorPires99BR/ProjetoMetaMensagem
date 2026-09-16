using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ObtemAgendamento
{
    public class ObtemAgendamentoValidator : AbstractValidator<ObtemAgendamentoCommand>
    {
        public ObtemAgendamentoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Agendamento não identificado.");
        }
    }
}
