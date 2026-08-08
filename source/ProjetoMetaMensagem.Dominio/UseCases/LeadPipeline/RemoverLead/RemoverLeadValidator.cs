using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.RemoverLead
{
    public class RemoverLeadValidator : AbstractValidator<RemoverLeadCommand>
    {
        public RemoverLeadValidator()
        {
            RuleFor(x => x.LeadId)
                .NotEmpty().WithMessage("Informe o lead que será removido.");
        }
    }
}
