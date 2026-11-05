using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaDisparosLote
{
    public class ListaDisparosLoteValidator : AbstractValidator<ListaDisparosLoteCommand>
    {
        public ListaDisparosLoteValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Informe a empresa.").When(x => !x.LoteId.HasValue);
        }
    }
}
