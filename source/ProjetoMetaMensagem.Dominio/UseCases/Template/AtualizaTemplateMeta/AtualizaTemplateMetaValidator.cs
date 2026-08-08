using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta
{
    public class AtualizaTemplateMetaValidator : AbstractValidator<AtualizaTemplateMetaCommand>
    {
        public AtualizaTemplateMetaValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa para sincronizar os templates com a Meta.");
        }
    }
}
