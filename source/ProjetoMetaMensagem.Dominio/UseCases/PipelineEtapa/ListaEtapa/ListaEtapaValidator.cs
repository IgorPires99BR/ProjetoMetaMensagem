using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa
{
    public class ListaEtapaValidator : AbstractValidator<ListaEtapaCommand>
    {
        public ListaEtapaValidator()
        {
            RuleFor(x => x.PipelineId)
                .NotEmpty().WithMessage("Informe o pipeline desejado.");
        }
    }
}
