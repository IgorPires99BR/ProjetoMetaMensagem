using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.DeletaParametro
{
    public class DeletaParametroValidator : AbstractValidator<DeletaParametroCommand>
    {
        public DeletaParametroValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o parâmetro que será excluído.");
        }
    }
}
