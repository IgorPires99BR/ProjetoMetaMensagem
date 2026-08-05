using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes
{
    public class ListaTemplateConexoesValidator : AbstractValidator<ListaTemplateConexoesCommand>
    {
        public ListaTemplateConexoesValidator()
        {
            RuleFor(x => x.EmpresaId).NotEqual(Guid.Empty).WithMessage("EmpresaId inválido");
        }
    }
}
