using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    public class SalvaDadosBancariosValidator : AbstractValidator<SalvaDadosBancariosCommand>
    {
        public SalvaDadosBancariosValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Banco)
                .Equal(DadosBancariosEmpresa.CodigoItau).WithMessage("No momento só o Itaú (341) é suportado para cobrança via Pix.");

            RuleFor(x => x.Agencia)
                .Matches(@"^\d{4}$").WithMessage("A agência deve ter 4 dígitos, sem o dígito verificador.")
                .When(x => !string.IsNullOrWhiteSpace(x.Agencia));

            RuleFor(x => x.Conta)
                .Matches(@"^\d{1,12}$").WithMessage("A conta deve ter só números, sem o dígito.")
                .When(x => !string.IsNullOrWhiteSpace(x.Conta));

            RuleFor(x => x.ContaDigito)
                .Matches(@"^[0-9Xx]$").WithMessage("O dígito da conta deve ter 1 caractere.")
                .When(x => !string.IsNullOrWhiteSpace(x.ContaDigito));

            RuleFor(x => x.TitularNome)
                .MaximumLength(255).WithMessage("O nome do titular deve ter no máximo 255 caracteres.");

            RuleFor(x => x.TitularDocumento)
                .Matches(@"^(\d{11}|\d{14})$").WithMessage("O CPF/CNPJ do titular deve ter 11 ou 14 dígitos, só números.")
                .When(x => !string.IsNullOrWhiteSpace(x.TitularDocumento));

            RuleFor(x => x.TipoChavePix)
                .Must(t => DadosBancariosEmpresa.TiposChavePix.Contains(t))
                .WithMessage("Tipo de chave Pix inválido.")
                .When(x => !string.IsNullOrWhiteSpace(x.TipoChavePix));

            RuleFor(x => x.TipoChavePix)
                .NotEmpty().WithMessage("Informe o tipo da chave Pix.")
                .When(x => !string.IsNullOrWhiteSpace(x.ChavePix));

            RuleFor(x => x.ChavePix)
                .Must((cmd, chave) => ChavePixValida(cmd.TipoChavePix, chave!))
                .WithMessage(cmd => MensagemChaveInvalida(cmd.TipoChavePix))
                .When(x => !string.IsNullOrWhiteSpace(x.ChavePix) && DadosBancariosEmpresa.TiposChavePix.Contains(x.TipoChavePix));

            RuleFor(x => x.Ambiente)
                .Must(a => a == DadosBancariosEmpresa.AmbienteSandbox || a == DadosBancariosEmpresa.AmbienteProducao)
                .WithMessage("Ambiente inválido.");

            RuleFor(x => x.ClientId)
                .MaximumLength(100).WithMessage("O Client ID deve ter no máximo 100 caracteres.");

            RuleFor(x => x)
                .Must(x => string.IsNullOrWhiteSpace(x.CertificadoPem) == string.IsNullOrWhiteSpace(x.ChavePrivadaPem))
                .WithMessage("Envie o certificado (.crt) e a chave privada (.key) juntos.");

            // Ativar a cobranca exige o que o Pix precisa pra ser gerado; os segredos ja gravados
            // sao conferidos no handler, que e quem enxerga o registro atual.
            When(x => x.CobrancaPixAtiva, () =>
            {
                RuleFor(x => x.Agencia).NotEmpty().WithMessage("Informe a agência para ativar a cobrança via Pix.");
                RuleFor(x => x.Conta).NotEmpty().WithMessage("Informe a conta para ativar a cobrança via Pix.");
                RuleFor(x => x.ChavePix).NotEmpty().WithMessage("Informe a chave Pix para ativar a cobrança via Pix.");
                RuleFor(x => x.ClientId).NotEmpty().WithMessage("Informe o Client ID do Itaú para ativar a cobrança via Pix.");
            });
        }

        private static bool ChavePixValida(string? tipo, string chave) => tipo switch
        {
            "CNPJ" => Regex.IsMatch(chave, @"^\d{14}$"),
            "CPF" => Regex.IsMatch(chave, @"^\d{11}$"),
            "EMAIL" => chave.Length <= 77 && Regex.IsMatch(chave, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
            // Formato do DICT: +55 e DDD + numero, sem mascara.
            "TELEFONE" => Regex.IsMatch(chave, @"^\+55\d{10,11}$"),
            "ALEATORIA" => Guid.TryParse(chave, out _),
            _ => false
        };

        private static string MensagemChaveInvalida(string? tipo) => tipo switch
        {
            "CNPJ" => "A chave Pix CNPJ deve ter 14 dígitos, só números.",
            "CPF" => "A chave Pix CPF deve ter 11 dígitos, só números.",
            "EMAIL" => "Informe um e-mail válido como chave Pix.",
            "TELEFONE" => "A chave Pix telefone deve estar no formato +5511999998888.",
            "ALEATORIA" => "A chave aleatória deve estar no formato 123e4567-e89b-12d3-a456-426614174000.",
            _ => "Chave Pix inválida."
        };
    }
}
