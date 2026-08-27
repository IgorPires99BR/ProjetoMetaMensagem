using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente
{
    public class CriaContaClienteValidator : AbstractValidator<CriaContaClienteCommand>
    {
        public CriaContaClienteValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome do cliente.")
                .MaximumLength(255).WithMessage("O nome deve ter no máximo 255 caracteres.");

            // O e-mail e o login do cliente e o endereco por onde a senha chega: sem ele a
            // conta nasce sem ninguem conseguir entrar nela.
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail do cliente.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.");

            // Sem esta lista, um plano digitado errado (ou mandado direto na API) era gravado
            // como se existisse, e a conta ficava com um plano que nenhuma regra reconhece.
            // A tela usa um select, entao isto protege quem chama a API por fora.
            RuleFor(x => x.Plano)
                .NotEmpty().WithMessage("Escolha o plano do cliente.")
                // ApplyConditionTo.CurrentValidator porque, sem ele, o When vale para a cadeia
                // inteira e desliga tambem o NotEmpty acima -- plano vazio passaria a ser aceito.
                .Must(SerPlanoConhecido).WithMessage($"Plano inválido. Use um destes: {string.Join(", ", PlanosAceitos)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.Plano), ApplyConditionTo.CurrentValidator);
        }

        private static readonly string[] PlanosAceitos =
        {
            PlanoAssinatura.Starter,
            PlanoAssinatura.Pro,
            PlanoAssinatura.Enterprise
        };

        private static bool SerPlanoConhecido(string? plano) =>
            PlanosAceitos.Any(p => string.Equals(p, plano, StringComparison.OrdinalIgnoreCase));
    }
}
