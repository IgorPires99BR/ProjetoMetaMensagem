using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha
{
    public class ListaCampanhaValidator : AbstractValidator<ListaCampanhaCommand>
    {
        public ListaCampanhaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
