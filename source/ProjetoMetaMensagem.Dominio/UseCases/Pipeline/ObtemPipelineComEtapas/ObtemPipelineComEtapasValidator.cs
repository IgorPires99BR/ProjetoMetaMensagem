using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas
{
    public class ObtemPipelineComEtapasValidator : AbstractValidator<ObtemPipelineComEtapasCommand>
    {
        public ObtemPipelineComEtapasValidator()
        {
            RuleFor(x => x.PipelineId)
                .NotEmpty().WithMessage("Informe o pipeline desejado.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
