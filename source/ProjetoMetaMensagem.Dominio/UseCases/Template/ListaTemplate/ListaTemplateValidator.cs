using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate
{
    public class ListaTemplateValidator : AbstractValidator<ListaTemplateCommand>
    {
        public ListaTemplateValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa para listar os templates.");
        }
    }
}
