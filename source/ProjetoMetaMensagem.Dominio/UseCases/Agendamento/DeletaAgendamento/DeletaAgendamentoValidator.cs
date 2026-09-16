using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.DeletaAgendamento
{
    public class DeletaAgendamentoValidator : AbstractValidator<DeletaAgendamentoCommand>
    {
        public DeletaAgendamentoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Agendamento não identificado.");
        }
    }
}
