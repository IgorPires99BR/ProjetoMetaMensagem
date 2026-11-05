using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaUltimosDisparos
{
    public class ListaUltimosDisparosValidator : AbstractValidator<ListaUltimosDisparosCommand>
    {
        public ListaUltimosDisparosValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Informe a empresa.");
        }
    }
}
