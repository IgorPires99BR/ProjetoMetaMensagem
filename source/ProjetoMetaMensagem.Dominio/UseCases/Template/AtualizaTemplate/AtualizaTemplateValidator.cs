using FluentValidation;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate
{
    public class AtualizaTemplateValidator : AbstractValidator<AtualizaTemplateCommand>
    {
        public AtualizaTemplateValidator()
        {
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Informe o template a ser editado.");
            RuleFor(x => x.Conteudo).NotEmpty().WithMessage("Informe o conteúdo do corpo do template.");

            RuleFor(x => x.Categoria)
                .Must(c => TemplateComponentesValidationRules.CategoriasValidas.Contains(c))
                .WithMessage("Categoria inválida. Use MARKETING, UTILITY ou AUTHENTICATION.");

            TemplateComponentesValidationRules.Aplicar(this);
        }
    }
}
