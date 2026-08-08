using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta
{
    public class CriarTemplateMetaValidator : AbstractValidator<CriarTemplateMetaCommand>
    {
        public CriarTemplateMetaValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do template.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do template.")
                .MaximumLength(255).WithMessage("O nome do template deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Idioma)
                .NotEmpty().WithMessage("Informe o idioma do template.")
                .MaximumLength(10).WithMessage("O idioma deve ter no máximo 10 caracteres.");

            RuleFor(x => x.Categoria)
                .NotEmpty().WithMessage("Informe a categoria do template.")
                .MaximumLength(100).WithMessage("A categoria deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Componentes)
                .NotEmpty().WithMessage("O template precisa de ao menos um componente.");

            RuleForEach(x => x.Componentes).ChildRules(componente =>
            {
                componente.RuleFor(c => c.Tipo)
                    .NotEmpty().WithMessage("Todo componente do template precisa de um tipo.");
            });
        }
    }
}
