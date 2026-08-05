using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ExcluiTemplateConexao
{
    public class ExcluiTemplateConexaoValidator : AbstractValidator<ExcluiTemplateConexaoCommand>
    {
        public ExcluiTemplateConexaoValidator()
        {
            RuleFor(x => x.Id).NotEqual(Guid.Empty).WithMessage("Id inválido");
        }
    }
}
