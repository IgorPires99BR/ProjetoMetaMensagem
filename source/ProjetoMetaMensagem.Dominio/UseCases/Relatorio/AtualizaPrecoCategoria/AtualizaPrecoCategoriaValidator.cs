using FluentValidation;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.AtualizaPrecoCategoria
{
    public class AtualizaPrecoCategoriaValidator : AbstractValidator<AtualizaPrecoCategoriaCommand>
    {
        private static readonly string[] CategoriasValidas = { "MARKETING", "UTILITY", "AUTHENTICATION" };

        public AtualizaPrecoCategoriaValidator()
        {
            RuleFor(x => x.Categoria)
                .NotEmpty().WithMessage("A categoria é obrigatória.")
                .Must(c => CategoriasValidas.Contains(c))
                .WithMessage("Categoria inválida. Use MARKETING, UTILITY ou AUTHENTICATION.");

            RuleFor(x => x.PrecoUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser negativo.");
        }
    }
}
