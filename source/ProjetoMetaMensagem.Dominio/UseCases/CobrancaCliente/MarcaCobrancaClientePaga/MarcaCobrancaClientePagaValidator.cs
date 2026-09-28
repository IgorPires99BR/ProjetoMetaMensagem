using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga
{
    public class MarcaCobrancaClientePagaValidator : AbstractValidator<MarcaCobrancaClientePagaCommand>
    {
        public MarcaCobrancaClientePagaValidator()
        {
            RuleFor(x => x.CobrancaClienteId).NotEmpty().WithMessage("Informe a cobrança.");

            // A tela deixa escolher a data real do pagamento; data futura so pode ser erro de digitacao.
            RuleFor(x => x.DataPagamento)
                .LessThan(_ => DateTime.Today.AddDays(1))
                .When(x => x.DataPagamento.HasValue)
                .WithMessage("A data do pagamento não pode ser no futuro.");
        }
    }
}
