using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    public class ProcessaAgendamentoValidator : AbstractValidator<ProcessaAgendamentoCommand>
    {
        public ProcessaAgendamentoValidator()
        {
            RuleFor(c => c.AgendamentoId).NotEmpty().WithMessage("O agendamento é obrigatório.");
        }
    }
}
