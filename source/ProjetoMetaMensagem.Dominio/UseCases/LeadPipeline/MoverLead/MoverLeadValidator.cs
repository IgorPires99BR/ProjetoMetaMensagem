using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead
{
    public class MoverLeadValidator : AbstractValidator<MoverLeadCommand>
    {
        public MoverLeadValidator()
        {
            RuleFor(x => x.LeadId)
                .NotEmpty().WithMessage("Informe o lead que será movido.");

            RuleFor(x => x.NovaEtapaId)
                .NotEmpty().WithMessage("Informe a etapa de destino.");
        }
    }
}
