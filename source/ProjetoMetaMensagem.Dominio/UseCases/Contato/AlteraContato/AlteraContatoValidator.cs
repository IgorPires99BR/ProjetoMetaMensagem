using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoValidator : AbstractValidator<AlteraContatoCommand>
    {
        public AlteraContatoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Não foi possível identificar o contato que será alterado.");

            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário responsável pelo contato.");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("Informe o telefone do contato.")
                .MaximumLength(50).WithMessage("O telefone deve ter no máximo 50 caracteres.");

            RuleFor(x => x.NomeContato)
                .MaximumLength(255).WithMessage("O nome do contato deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.NomeCliente)
                .MaximumLength(255).WithMessage("O nome do cliente deve ter no máximo 255 caracteres.");

            RuleFor(x => x.DiaVencimento)
                .InclusiveBetween(1, 31).WithMessage("O dia de vencimento deve estar entre 1 e 31.")
                .When(x => x.DiaVencimento.HasValue);

            RuleFor(x => x.TaxaJuros)
                .GreaterThanOrEqualTo(0).WithMessage("A taxa de juros não pode ser negativa.")
                .When(x => x.TaxaJuros.HasValue);

            RuleFor(x => x.TaxaJurosMensal)
                .GreaterThanOrEqualTo(0).WithMessage("A taxa de juros mensal não pode ser negativa.")
                .When(x => x.TaxaJurosMensal.HasValue);

            RuleFor(x => x.ValorFatura)
                .GreaterThanOrEqualTo(0).WithMessage("O valor da fatura não pode ser negativo.")
                .When(x => x.ValorFatura.HasValue);
        }
    }
}
