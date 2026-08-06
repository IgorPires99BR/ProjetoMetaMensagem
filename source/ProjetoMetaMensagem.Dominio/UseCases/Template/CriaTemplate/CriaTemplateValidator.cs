using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateValidator : AbstractValidator<CriaTemplateCommand>
    {
        private static readonly string[] TiposHeaderValidos = { "TEXT", "IMAGE", "VIDEO", "DOCUMENT" };

        public CriaTemplateValidator()
        {
            RuleFor(x => x.HeaderTipo)
                .Must(t => string.IsNullOrEmpty(t) || t == "NONE" || TiposHeaderValidos.Contains(t))
                .WithMessage("Tipo de cabeçalho inválido.");

            RuleFor(x => x.HeaderTexto)
                .NotEmpty()
                .When(x => x.HeaderTipo == "TEXT")
                .WithMessage("Informe o texto do cabeçalho.");

            RuleFor(x => x.HeaderExemploHandle)
                .NotEmpty()
                .When(x => x.HeaderTipo == "IMAGE" || x.HeaderTipo == "VIDEO" || x.HeaderTipo == "DOCUMENT")
                .WithMessage("Envie um arquivo de exemplo para o cabeçalho de mídia antes de cadastrar o template.");

            RuleFor(x => x.ExemplosBody)
                .Must((command, exemplos) =>
                {
                    var quantidadeVariaveis = Regex.Matches(command.Conteudo ?? string.Empty, @"\{\{\d+\}\}").Count;
                    if (quantidadeVariaveis == 0) return true;
                    return exemplos != null && exemplos.Count >= quantidadeVariaveis && exemplos.All(e => !string.IsNullOrWhiteSpace(e));
                })
                .WithMessage("Preencha um valor de exemplo para cada variável do corpo do template.");

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
