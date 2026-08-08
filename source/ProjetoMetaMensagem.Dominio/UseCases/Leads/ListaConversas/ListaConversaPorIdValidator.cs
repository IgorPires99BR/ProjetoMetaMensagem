using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas
{
    public class ListaConversaPorIdValidator : AbstractValidator<ListaConversaPorIdCommand>
    {
        public ListaConversaPorIdValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
