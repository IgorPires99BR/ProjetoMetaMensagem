using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.SimulaPagamentoSandbox
{
    public class SimulaPagamentoSandboxValidator : AbstractValidator<SimulaPagamentoSandboxCommand>
    {
        public SimulaPagamentoSandboxValidator()
        {
            RuleFor(x => x.CobrancaClienteId).NotEmpty().WithMessage("Informe a cobrança.");
        }
    }
}
