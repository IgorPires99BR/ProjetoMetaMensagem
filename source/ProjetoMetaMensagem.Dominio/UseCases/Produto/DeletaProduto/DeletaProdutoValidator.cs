using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto
{
    public class DeletaProdutoValidator : AbstractValidator<DeletaProdutoCommand>
    {
        public DeletaProdutoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id é obrigatório.");
        }
    }
}
