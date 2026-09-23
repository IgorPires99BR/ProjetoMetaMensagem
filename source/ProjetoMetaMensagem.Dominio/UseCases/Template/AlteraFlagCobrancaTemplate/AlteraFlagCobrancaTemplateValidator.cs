using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraFlagCobrancaTemplate
{
    public class AlteraFlagCobrancaTemplateValidator : AbstractValidator<AlteraFlagCobrancaTemplateCommand>
    {
        public AlteraFlagCobrancaTemplateValidator()
        {
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Informe o template.");
        }
    }
}
