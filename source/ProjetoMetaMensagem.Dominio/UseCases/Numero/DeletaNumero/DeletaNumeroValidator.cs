using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero
{
    public class DeletaNumeroValidator : AbstractValidator<DeletaNumeroCommand>
    {
        public DeletaNumeroValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o número que será excluído.");
        }
    }
}
