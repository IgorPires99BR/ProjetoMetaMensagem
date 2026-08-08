using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead
{
    public class AdicionarLeadValidator : AbstractValidator<AdicionarLeadCommand>
    {
        public AdicionarLeadValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do lead.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato a ser adicionado ao pipeline.");

            RuleFor(x => x.PipelineEtapaId)
                .NotEmpty().WithMessage("Informe a etapa inicial do lead.");
        }
    }
}
