using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.EnviaRecorrenciaCobranca
{
    public class EnviaRecorrenciaCobrancaValidator : AbstractValidator<EnviaRecorrenciaCobrancaCommand>
    {
        public EnviaRecorrenciaCobrancaValidator()
        {
            RuleFor(x => x.CobrancaClienteId).NotEmpty().WithMessage("Informe a cobrança.");
        }
    }
}
