using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateValidator : AbstractValidator<CriaTemplateCommand>
    {
        public CriaTemplateValidator()
        {
            RuleFor(x => x.Botoes)
                .Must(b => b == null || b.Count <= 3)
                .WithMessage("A Meta permite no máximo 3 botões por template.");

            RuleFor(x => x.Botoes)
                .Must(b => b == null || b.Count(x => x.Tipo == "PHONE_NUMBER") <= 1)
                .WithMessage("Só é permitido 1 botão de telefone por template.");

            RuleForEach(x => x.Botoes).ChildRules(botao =>
            {
                botao.RuleFor(b => b.Texto).NotEmpty().WithMessage("Todo botão precisa de um texto.");
                botao.RuleFor(b => b.Url)
                    .NotEmpty()
                    .When(b => b.Tipo == "URL")
                    .WithMessage("Botões do tipo URL precisam de um link.");
                botao.RuleFor(b => b.NumeroTelefone)
                    .NotEmpty()
                    .When(b => b.Tipo == "PHONE_NUMBER")
                    .WithMessage("Botões do tipo telefone precisam de um número.");
            });
        }
    }
}
