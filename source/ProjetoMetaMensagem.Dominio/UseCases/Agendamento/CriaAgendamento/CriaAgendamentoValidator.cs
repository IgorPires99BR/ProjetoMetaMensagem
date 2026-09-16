using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.CriaAgendamento
{
    public class CriaAgendamentoValidator : AbstractValidator<CriaAgendamentoCommand>
    {
        public CriaAgendamentoValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Não foi possível identificar a empresa do agendamento.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("Informe o nome do agendamento.")
                .MaximumLength(200).WithMessage("O nome do agendamento deve ter no máximo 200 caracteres.");
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Selecione o modelo de mensagem a enviar.");
            RuleFor(x => x.TipoRecorrencia)
                .Must(t => t == Entidades.Agendamento.Diaria || t == Entidades.Agendamento.Semanal || t == Entidades.Agendamento.Mensal)
                .WithMessage("Informe uma recorrência válida (diária, semanal ou mensal).");
            RuleFor(x => x.DataInicio).NotEmpty().WithMessage("Informe a data e hora de início do agendamento.");
            RuleFor(x => x.DataFim)
                .GreaterThan(x => x.DataInicio).WithMessage("A data de término deve ser posterior à data de início.")
                .When(x => x.DataFim.HasValue);
            RuleFor(x => x.ContatoIds).NotEmpty().WithMessage("Selecione ao menos um contato para o agendamento.");
        }
    }
}
