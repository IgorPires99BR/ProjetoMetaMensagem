using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.DevolverAoBot
{
    public class DevolverAoBotValidator : AbstractValidator<DevolverAoBotCommand>
    {
        public DevolverAoBotValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato.");
        }
    }
}
