using FluentValidation;
using ProjetoMetaMensagem.Dominio.Servicos;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.AlteraAgendamento
{
    public class AlteraAgendamentoValidator : AbstractValidator<AlteraAgendamentoCommand>
    {
        public AlteraAgendamentoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Agendamento não identificado.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("Informe o nome do agendamento.")
                .MaximumLength(200).WithMessage("O nome do agendamento deve ter no máximo 200 caracteres.");
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Selecione o modelo de mensagem a enviar.");
            RuleFor(x => x.TipoRecorrencia)
                .Must(t => t == Entidades.Agendamento.Diaria || t == Entidades.Agendamento.Semanal
                    || t == Entidades.Agendamento.Mensal || t == Entidades.Agendamento.VencimentoContato)
                .WithMessage("Informe uma recorrência válida (diária, semanal, mensal ou por dia de vencimento do contato).");
            RuleFor(x => x.DataInicio).NotEmpty().WithMessage("Informe a data de início da vigência do agendamento.");
            RuleFor(x => x.DataFim)
                .GreaterThan(x => x.DataInicio).WithMessage("A data de término deve ser posterior à data de início.")
                .When(x => x.DataFim.HasValue);
            RuleFor(x => x.DataReferencia).NotEmpty().WithMessage("Informe a data e hora de referência do disparo.");
            RuleFor(x => x.DataReferencia)
                .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data e hora de referência não pode ser anterior à data de início.");
            RuleFor(x => x.DataReferencia)
                .LessThanOrEqualTo(x => x.DataFim!.Value).WithMessage("A data e hora de referência não pode ser posterior à data de término.")
                .When(x => x.DataFim.HasValue);
            RuleFor(x => x.ContatoIds).NotEmpty().WithMessage("Selecione ao menos um contato para o agendamento.");
            RuleFor(x => x.DiasSemana)
                .Must(dias => dias.All(d => d >= 0 && d <= 6))
                .WithMessage("Dia da semana inválido.")
                .When(x => x.DiasSemana != null);
            RuleFor(x => x.DiaDoMes)
                .InclusiveBetween(1, 31).WithMessage("O dia do mês deve estar entre 1 e 31.")
                .When(x => x.DiaDoMes.HasValue);
            RuleFor(x => x.Variaveis)
                .Must(v => v.All(i => ResolvedorDeVariaveis.OrigemValida(i.Origem)))
                .WithMessage("Há uma variável com origem inválida.")
                .Must(v => v.All(i => i.Origem != ResolvedorDeVariaveis.ParametroCadastrado || i.ParametroId.HasValue))
                .WithMessage("Selecione o parâmetro de cada variável que usa um parâmetro cadastrado.")
                .When(x => x.Variaveis != null);
        }
    }
}
