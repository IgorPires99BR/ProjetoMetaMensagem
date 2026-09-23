using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga
{
    public class MarcaCobrancaClientePagaValidator : AbstractValidator<MarcaCobrancaClientePagaCommand>
    {
        public MarcaCobrancaClientePagaValidator()
        {
            RuleFor(x => x.CobrancaClienteId).NotEmpty().WithMessage("Informe a cobrança.");
        }
    }
}
