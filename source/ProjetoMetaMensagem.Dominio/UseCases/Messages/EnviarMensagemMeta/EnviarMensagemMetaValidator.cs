using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaValidator : AbstractValidator<EnviarMensagemMetaCommand>
    {
        public EnviarMensagemMetaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("Informe o celular de destino.");

            RuleFor(x => x.textoMensagem)
                .NotEmpty().WithMessage("Informe o texto da mensagem.");
        }
    }
}
