using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemProvedorCobranca
{
    public class ObtemProvedorCobrancaValidator : AbstractValidator<ObtemProvedorCobrancaCommand>
    {
        public ObtemProvedorCobrancaValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
