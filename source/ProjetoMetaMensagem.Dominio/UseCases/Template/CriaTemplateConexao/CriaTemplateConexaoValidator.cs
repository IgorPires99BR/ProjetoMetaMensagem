using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao
{
    public class CriaTemplateConexaoValidator : AbstractValidator<CriaTemplateConexaoCommand>
    {
        public CriaTemplateConexaoValidator()
        {
            RuleFor(x => x.EmpresaId).NotEqual(Guid.Empty).WithMessage("EmpresaId inválido");
            RuleFor(x => x.TemplateOrigemId).NotEqual(Guid.Empty).WithMessage("TemplateOrigemId inválido");
            RuleFor(x => x.TemplateDestinoId).NotEqual(Guid.Empty).WithMessage("TemplateDestinoId inválido");
            RuleFor(x => x.BotaoTexto).MaximumLength(100).WithMessage("BotaoTexto deve ter no máximo 100 caracteres");
        }
    }
}
