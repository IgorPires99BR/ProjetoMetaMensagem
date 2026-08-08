using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha
{
    public class CriaCampanhaValidator : AbstractValidator<CriaCampanhaCommand>
    {
        public CriaCampanhaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa da campanha.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome da campanha.")
                .MaximumLength(150).WithMessage("O nome da campanha deve ter no máximo 150 caracteres.");

            RuleFor(x => x.DataAgendamento)
                .NotEmpty().WithMessage("Informe a data de agendamento do disparo.");

            RuleFor(x => x.ContatoIds)
                .NotEmpty().WithMessage("Selecione ao menos um contato para a campanha.");

            RuleFor(x => x.ConteudoLivre)
                .NotEmpty().WithMessage("Informe o template ou o conteúdo da mensagem.")
                .When(x => x.TemplateId == null);
        }
    }
}
