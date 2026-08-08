using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag
{
    public class ListaTagValidator : AbstractValidator<ListaTagCommand>
    {
        public ListaTagValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
