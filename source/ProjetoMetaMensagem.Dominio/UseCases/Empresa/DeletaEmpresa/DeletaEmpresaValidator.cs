using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa
{
    public class DeletaEmpresaValidator : AbstractValidator<DeletaEmpresaCommand>
    {
        public DeletaEmpresaValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Informe a empresa que será excluída.");

            RuleFor(x => x.IdEmpresa)
                .Must(id => Guid.TryParse(id, out _)).WithMessage("Identificador da empresa inválido.")
                .When(x => !string.IsNullOrWhiteSpace(x.IdEmpresa));
        }
    }
}
