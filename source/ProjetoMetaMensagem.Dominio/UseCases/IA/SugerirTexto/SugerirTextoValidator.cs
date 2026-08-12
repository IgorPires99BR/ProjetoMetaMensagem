using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.IA.SugerirTexto
{
    public class SugerirTextoValidator : AbstractValidator<SugerirTextoCommand>
    {
        public SugerirTextoValidator()
        {
            RuleFor(x => x.Instrucao)
                .NotEmpty().WithMessage("Informe o que a IA deve sugerir.");

            RuleFor(x => x.Quantidade)
                .InclusiveBetween(1, 5).WithMessage("Quantidade de alternativas deve ser entre 1 e 5.");
        }
    }
}
