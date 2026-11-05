using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.AlteraAtivoCobrancaCliente
{
    public class AlteraAtivoCobrancaClienteValidator : AbstractValidator<AlteraAtivoCobrancaClienteCommand>
    {
        public AlteraAtivoCobrancaClienteValidator()
        {
            RuleFor(x => x.CobrancaClienteId).NotEmpty().WithMessage("Informe a cobrança.");
        }
    }
}
