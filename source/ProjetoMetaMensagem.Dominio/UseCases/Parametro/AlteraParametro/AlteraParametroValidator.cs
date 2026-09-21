using FluentValidation;
using ProjetoMetaMensagem.Dominio.Servicos;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.AlteraParametro
{
    public class AlteraParametroValidator : AbstractValidator<AlteraParametroCommand>
    {
        public AlteraParametroValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o parâmetro que será alterado.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do parâmetro.")
                .MaximumLength(100).WithMessage("O nome do parâmetro deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("A descrição deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Tipo)
                .Must(t => t == Entidades.Parametro.Fixo || t == Entidades.Parametro.CampoDoContato)
                .WithMessage("Informe um tipo de parâmetro válido (fixo ou campo do contato).");

            RuleFor(x => x.Valor)
                .NotEmpty().WithMessage("Informe o valor do parâmetro.")
                .MaximumLength(500).WithMessage("O valor do parâmetro deve ter no máximo 500 caracteres.");

            RuleFor(x => x.Valor)
                .Must(v => ResolvedorDeVariaveis.CamposDoContato.Contains(v))
                .WithMessage("Escolha um campo do contato válido.")
                .When(x => x.Tipo == Entidades.Parametro.CampoDoContato);
        }
    }
}
