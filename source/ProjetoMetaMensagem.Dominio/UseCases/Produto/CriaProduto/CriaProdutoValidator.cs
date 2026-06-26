using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto
{
    public class CriaProdutoValidator : AbstractValidator<CriaProdutoCommand>
    {
        public CriaProdutoValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("EmpresaId é obrigatório.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(255).WithMessage("Nome deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Preco)
                .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

            RuleFor(x => x.Descricao)
                .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.");

            RuleFor(x => x.ImagemUrl)
                .MaximumLength(500).WithMessage("ImagemUrl deve ter no máximo 500 caracteres.");

            RuleFor(x => x.LinkUrl)
                .MaximumLength(500).WithMessage("LinkUrl deve ter no máximo 500 caracteres.");

            RuleFor(x => x.Categoria)
                .MaximumLength(100).WithMessage("Categoria deve ter no máximo 100 caracteres.");
        }
    }
}
