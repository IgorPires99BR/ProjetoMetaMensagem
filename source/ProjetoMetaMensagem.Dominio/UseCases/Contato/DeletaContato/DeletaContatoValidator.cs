using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato
{
    public class DeletaContatoValidator : AbstractValidator<DeletaContatoCommand>
    {
        public DeletaContatoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Informe o contato que será excluído.");

            RuleFor(x => x.Id)
                .Must(id => Guid.TryParse(id, out _)).WithMessage("Identificador do contato inválido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Id));
        }
    }
}
