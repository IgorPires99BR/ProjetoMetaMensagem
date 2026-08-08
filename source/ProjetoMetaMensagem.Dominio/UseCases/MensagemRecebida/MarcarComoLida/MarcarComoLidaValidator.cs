using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida
{
    public class MarcarComoLidaValidator : AbstractValidator<MarcarComoLidaCommand>
    {
        public MarcarComoLidaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato.");
        }
    }
}
