using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto
{
    public class AlteraProdutoValidator : AbstractValidator<AlteraProdutoCommand>
    {
        public AlteraProdutoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id é obrigatório.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("EmpresaId é obrigatório.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .Length(1, 255).WithMessage("Nome deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Preco)
                .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

            RuleFor(x => x.Descricao)
                .Length(0, 1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.");

            RuleFor(x => x.ImagemUrl)
                .Length(0, 500).WithMessage("ImagemUrl deve ter no máximo 500 caracteres.");

            RuleFor(x => x.LinkUrl)
                .Length(0, 500).WithMessage("LinkUrl deve ter no máximo 500 caracteres.");

            RuleFor(x => x.Categoria)
                .Length(0, 100).WithMessage("Categoria deve ter no máximo 100 caracteres.");
        }
    }
}
