using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta
{
    public class EnviarMidiaMetaValidator : AbstractValidator<EnviarMidiaMetaCommand>
    {
        public EnviarMidiaMetaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato de destino.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("Informe o celular de destino.");

            RuleFor(x => x.Arquivo)
                .NotEmpty().WithMessage("Arquivo não informado.");

            RuleFor(x => x.MimeType)
                .NotEmpty().WithMessage("Não foi possível identificar o tipo do arquivo.");

            RuleFor(x => x.TipoMidia)
                .NotEmpty().WithMessage("Informe o tipo de mídia.");
        }
    }
}
