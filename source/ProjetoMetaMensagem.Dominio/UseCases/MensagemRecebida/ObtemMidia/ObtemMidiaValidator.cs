using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia
{
    public class ObtemMidiaValidator : AbstractValidator<ObtemMidiaCommand>
    {
        public ObtemMidiaValidator()
        {
            RuleFor(x => x.MidiaId)
                .NotEmpty().WithMessage("Informe a mídia desejada.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
