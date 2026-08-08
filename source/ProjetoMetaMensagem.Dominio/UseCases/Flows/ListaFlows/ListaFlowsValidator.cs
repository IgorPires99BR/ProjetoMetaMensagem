using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows
{
    public class ListaFlowsValidator : AbstractValidator<ListaFlowsCommand>
    {
        public ListaFlowsValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
