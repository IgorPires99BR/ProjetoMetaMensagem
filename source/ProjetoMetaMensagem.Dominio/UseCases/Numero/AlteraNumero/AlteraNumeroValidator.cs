using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero
{
    public class AlteraNumeroValidator : AbstractValidator<AlteraNumeroCommand>
    {
        public AlteraNumeroValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Não foi possível identificar o número que será alterado.");

            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário responsável pelo número.");

            RuleFor(x => x.NumeroTelefone)
                .NotEmpty().WithMessage("Informe o número de telefone.")
                .MaximumLength(50).WithMessage("O número de telefone deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(100).WithMessage("A descrição deve ter no máximo 100 caracteres.");

            RuleFor(x => x.InstanciaId)
                .MaximumLength(255).WithMessage("O identificador da instância deve ter no máximo 255 caracteres.");
        }
    }
}
