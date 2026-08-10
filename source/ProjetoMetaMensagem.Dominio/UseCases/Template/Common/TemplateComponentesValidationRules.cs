using FluentValidation;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.Common
{
    // Regras de validação dos componentes (Header/Body/Footer/Botões) compartilhadas entre
    // criação (CriaTemplateValidator) e edição (AtualizaTemplateValidator) de template, para não
    // duplicar a mesma lógica nos dois validators.
    public static class TemplateComponentesValidationRules
    {
        private static readonly string[] TiposHeaderValidos = { "TEXT", "IMAGE", "VIDEO", "DOCUMENT" };
        public static readonly string[] CategoriasValidas = { "MARKETING", "UTILITY", "AUTHENTICATION" };

        public static void Aplicar<T>(AbstractValidator<T> validator) where T : ITemplateComponentesInput
        {
            validator.RuleFor(x => x.HeaderTipo)
                .Must(t => string.IsNullOrEmpty(t) || t == "NONE" || TiposHeaderValidos.Contains(t))
                .WithMessage("Tipo de cabeçalho inválido.");

            validator.RuleFor(x => x.HeaderTexto)
                .NotEmpty()
                .When(x => x.HeaderTipo == "TEXT")
                .WithMessage("Informe o texto do cabeçalho.");

            validator.RuleFor(x => x.HeaderExemploHandle)
                .NotEmpty()
                .When(x => x.HeaderTipo == "IMAGE" || x.HeaderTipo == "VIDEO" || x.HeaderTipo == "DOCUMENT")
                .WithMessage("Envie um arquivo de exemplo para o cabeçalho de mídia antes de cadastrar o template.");

            validator.RuleFor(x => x.FooterTexto)
                .MaximumLength(60)
                .WithMessage("O rodapé pode ter no máximo 60 caracteres.");

            validator.RuleFor(x => x.ExemplosBody)
                .Must((command, exemplos) =>
                {
                    var quantidadeVariaveis = Regex.Matches(command.Conteudo ?? string.Empty, @"\{\{\d+\}\}").Count;
                    if (quantidadeVariaveis == 0) return true;
                    return exemplos != null && exemplos.Count >= quantidadeVariaveis && exemplos.All(e => !string.IsNullOrWhiteSpace(e));
                })
                .WithMessage("Preencha um valor de exemplo para cada variável do corpo do template.");

            validator.RuleFor(x => x.Botoes)
                .Must(b => b == null || b.Count <= 3)
                .WithMessage("A Meta permite no máximo 3 botões por template.");

            validator.RuleFor(x => x.Botoes)
                .Must(b => b == null || b.Count(x => x.Tipo == "PHONE_NUMBER") <= 1)
                .WithMessage("Só é permitido 1 botão de telefone por template.");

            validator.RuleFor(x => x.Botoes)
                .Must(b => b == null || b.Count(x => x.Tipo == "COPY_CODE") <= 1)
                .WithMessage("Só é permitido 1 botão de código de cupom por template.");

            validator.RuleForEach(x => x.Botoes).ChildRules(botao =>
            {
                botao.RuleFor(b => b.Texto)
                    .NotEmpty()
                    .When(b => b.Tipo != "COPY_CODE")
                    .WithMessage("Todo botão precisa de um texto.");
                botao.RuleFor(b => b.Url)
                    .NotEmpty()
                    .When(b => b.Tipo == "URL")
                    .WithMessage("Botões do tipo URL precisam de um link.");
                botao.RuleFor(b => b.NumeroTelefone)
                    .NotEmpty()
                    .When(b => b.Tipo == "PHONE_NUMBER")
                    .WithMessage("Botões do tipo telefone precisam de um número.");
                botao.RuleFor(b => b.CodigoExemplo)
                    .NotEmpty()
                    .When(b => b.Tipo == "COPY_CODE")
                    .WithMessage("Botões de código de cupom precisam de um código de exemplo.");
                botao.RuleFor(b => b.CodigoExemplo)
                    .MaximumLength(15)
                    .WithMessage("O código de exemplo do cupom pode ter no máximo 15 caracteres.");
            });
        }
    }
}
