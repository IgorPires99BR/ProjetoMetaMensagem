using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaExecucoesAgendamento
{
    public class ListaExecucoesAgendamentoValidator : AbstractValidator<ListaExecucoesAgendamentoCommand>
    {
        public ListaExecucoesAgendamentoValidator()
        {
            RuleFor(x => x.AgendamentoId).NotEmpty().WithMessage("Agendamento não identificado.");
        }
    }
}
