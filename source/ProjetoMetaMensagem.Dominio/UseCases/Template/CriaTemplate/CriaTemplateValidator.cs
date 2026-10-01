using FluentValidation;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateValidator : AbstractValidator<CriaTemplateCommand>
    {
        public CriaTemplateValidator()
        {
            RuleFor(x => x.NomeTemplate).NotEmpty().WithMessage("Informe o nome do template.");
            RuleFor(x => x.NomeExibicao).MaximumLength(255).WithMessage("O nome do modelo pode ter no máximo 255 caracteres.");
            RuleFor(x => x.Conteudo).NotEmpty().WithMessage("Informe o conteúdo do corpo do template.");

            RuleFor(x => x.Categoria)
                .Must(c => TemplateComponentesValidationRules.CategoriasValidas.Contains(c))
                .WithMessage("Categoria inválida. Use MARKETING, UTILITY ou AUTHENTICATION.");

            TemplateComponentesValidationRules.Aplicar(this);
        }
    }
}
