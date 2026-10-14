using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemSituacaoSubconta
{
    public class ObtemSituacaoSubcontaValidator : AbstractValidator<ObtemSituacaoSubcontaCommand>
    {
        public ObtemSituacaoSubcontaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
