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
            RuleFor(x => x.NomeExibicao).MaximumLength(255).WithMessage("O nome do modelo pode ter no máximo 255 caracteres.");
            RuleFor(x => x.Conteudo).NotEmpty().WithMessage("Informe o conteúdo do corpo do template.");

            RuleFor(x => x.Categoria)
                .Must(c => TemplateComponentesValidationRules.CategoriasValidas.Contains(c))
                .WithMessage("Categoria inválida. Use MARKETING, UTILITY ou AUTHENTICATION.");

            TemplateComponentesValidationRules.AplicarEstrutura(this);
        }
    }

    // Roda so quando o conteudo mudou e a edicao vai ser reenviada pra Meta (ver AplicarExemplos).
    public class AtualizaTemplateEnvioMetaValidator : AbstractValidator<AtualizaTemplateCommand>
    {
        public AtualizaTemplateEnvioMetaValidator()
        {
            TemplateComponentesValidationRules.AplicarExemplos(this);
        }
    }
}
