using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto
{
    public class ListaProdutoValidator : AbstractValidator<ListaProdutoCommand>
    {
        public ListaProdutoValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("EmpresaId é obrigatório.");
        }
    }
}
