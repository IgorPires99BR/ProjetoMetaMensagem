using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.SalvaGatilhosTemplate
{
    public class SalvaGatilhosTemplateValidator : AbstractValidator<SalvaGatilhosTemplateCommand>
    {
        public SalvaGatilhosTemplateValidator()
        {
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Informe o template.");
            RuleFor(x => x.Gatilhos).NotNull().WithMessage("Informe os momentos de disparo.");

            RuleFor(x => x.Gatilhos)
                .Must(g => g == null || g.Select(i => i.Tipo).Distinct().Count() == g.Count)
                .WithMessage("Cada momento de disparo só pode aparecer uma vez no template.");

            RuleForEach(x => x.Gatilhos).ChildRules(g =>
            {
                g.RuleFor(i => i.Tipo)
                    .Must(t => TemplateGatilho.Tipos.Contains(t))
                    .WithMessage("Momento de disparo inválido.");

                g.RuleFor(i => i.DiasAtraso)
                    .NotNull().InclusiveBetween(0, 365)
                    .When(i => i.Tipo == TemplateGatilho.AtrasoPagamento)
                    .WithMessage("Informe com quantos dias de atraso (0 a 365) a mensagem deve sair.");

                g.RuleForEach(i => i.Variaveis)
                    .Must(v => ResolvedorDeVariaveis.OrigemValida(v.Origem))
                    .WithMessage("Origem de variável inválida.");

                g.RuleForEach(i => i.Variaveis)
                    .Must(v => v.Origem != ResolvedorDeVariaveis.ParametroCadastrado || v.ParametroId.HasValue)
                    .WithMessage("Selecione o parâmetro de cada variável que usa um parâmetro cadastrado.");

                g.RuleForEach(i => i.Variaveis)
                    .Must(v => v.Origem != ResolvedorDeVariaveis.Fixo || !string.IsNullOrWhiteSpace(v.ValorFixo))
                    .WithMessage("Preencha o texto de cada variável de texto fixo.");
            });
        }
    }
}
