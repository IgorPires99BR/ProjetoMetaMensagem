using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoValidator : AbstractValidator<CriaContatoCommand>
    {
        public CriaContatoValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário responsável pelo contato.");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("Informe o telefone do contato.")
                .MaximumLength(50).WithMessage("O telefone deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Nome)
                .MaximumLength(255).WithMessage("O nome do contato deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
