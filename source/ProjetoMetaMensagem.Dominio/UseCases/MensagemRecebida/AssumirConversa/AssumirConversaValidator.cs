using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.AssumirConversa
{
    public class AssumirConversaValidator : AbstractValidator<AssumirConversaCommand>
    {
        public AssumirConversaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Informe o contato.");

            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário.");
        }
    }
}
