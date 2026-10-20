using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaGatilhosTemplate
{
    public class ListaGatilhosTemplateValidator : AbstractValidator<ListaGatilhosTemplateCommand>
    {
        public ListaGatilhosTemplateValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Informe a empresa.");
        }
    }
}
