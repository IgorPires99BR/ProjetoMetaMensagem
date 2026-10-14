using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.VinculaSubcontaAsaas
{
    public class VinculaSubcontaAsaasValidator : AbstractValidator<VinculaSubcontaAsaasCommand>
    {
        public VinculaSubcontaAsaasValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.WalletId)
                .NotEmpty().WithMessage("Informe o walletId da subconta Asaas.")
                .Matches(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
                .WithMessage("O walletId deve estar no formato xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.");
        }
    }
}
