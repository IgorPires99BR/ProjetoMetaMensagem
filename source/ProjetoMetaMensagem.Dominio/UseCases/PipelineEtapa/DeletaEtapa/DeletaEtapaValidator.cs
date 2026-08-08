using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.DeletaEtapa
{
    public class DeletaEtapaValidator : AbstractValidator<DeletaEtapaCommand>
    {
        public DeletaEtapaValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe a etapa que será excluída.");
        }
    }
}
