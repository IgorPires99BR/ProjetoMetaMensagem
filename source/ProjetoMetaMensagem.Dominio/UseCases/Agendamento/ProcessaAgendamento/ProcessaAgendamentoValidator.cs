using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Sem regras: o comando nao leva parametro (varre tudo). Mantido so pra seguir o mesmo
    // formato Command+Handler+Result+Validator do resto do Dominio.
    public class ProcessaAgendamentoValidator : AbstractValidator<ProcessaAgendamentoCommand>
    {
    }
}
