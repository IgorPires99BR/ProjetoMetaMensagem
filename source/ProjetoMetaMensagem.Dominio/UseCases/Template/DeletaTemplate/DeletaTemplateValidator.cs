using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateValidator : AbstractValidator<DeletaTemplateCommand>
    {
        public DeletaTemplateValidator()
        {
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Informe o template a ser excluído.");
        }
    }
}
