using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaAgendamento
{
    public class ListaAgendamentoValidator : AbstractValidator<ListaAgendamentoCommand>
    {
        public ListaAgendamentoValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
