using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens
{
    public class ListaRelatorioMensagensValidator : AbstractValidator<ListaRelatorioMensagensCommand>
    {
        public ListaRelatorioMensagensValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Pagina)
                .GreaterThanOrEqualTo(0).WithMessage("A página não pode ser negativa.");

            RuleFor(x => x.TamanhoPagina)
                .InclusiveBetween(1, 200).WithMessage("O tamanho da página deve estar entre 1 e 200.");

            RuleFor(x => x.DataFim)
                .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data final não pode ser anterior à data inicial.")
                .When(x => x.DataInicio.HasValue && x.DataFim.HasValue);
        }
    }
}
