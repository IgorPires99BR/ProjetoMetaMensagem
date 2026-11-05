using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraAtivoTemplate
{
    public class AlteraAtivoTemplateValidator : AbstractValidator<AlteraAtivoTemplateCommand>
    {
        public AlteraAtivoTemplateValidator()
        {
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Informe o template.");
        }
    }
}
