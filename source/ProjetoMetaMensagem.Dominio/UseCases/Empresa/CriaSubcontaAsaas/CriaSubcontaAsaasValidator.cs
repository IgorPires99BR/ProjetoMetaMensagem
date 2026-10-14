using FluentValidation;
using ProjetoMetaMensagem.Dominio.Helpers;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaSubcontaAsaas
{
    // Roda depois da normalizacao do handler (documento, celular e CEP ja so com digitos).
    public class CriaSubcontaAsaasValidator : AbstractValidator<CriaSubcontaAsaasCommand>
    {
        public static readonly string[] TiposEmpresa = { "MEI", "LIMITED", "INDIVIDUAL", "ASSOCIATION" };

        public CriaSubcontaAsaasValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome (razão social ou nome completo) do titular da subconta.")
                .MaximumLength(255).WithMessage("O nome deve ter no máximo 255 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail da subconta.")
                .EmailAddress().WithMessage("Informe um e-mail válido.");

            RuleFor(x => x.CpfCnpj)
                .NotEmpty().WithMessage("Informe o CPF ou CNPJ do titular da subconta.")
                .Must(DocumentoHelper.CpfCnpjValido).WithMessage("CPF/CNPJ inválido.");

            RuleFor(x => x.DataNascimento)
                .NotNull().WithMessage("Informe a data de nascimento do titular (obrigatória para CPF).")
                .LessThan(_ => DateTime.Today.AddYears(-18)).WithMessage("O titular da subconta precisa ser maior de idade.")
                .When(x => DocumentoHelper.EhCpf(x.CpfCnpj));

            RuleFor(x => x.TipoEmpresa)
                .Must(t => TiposEmpresa.Contains(t)).WithMessage("Escolha o tipo da empresa (MEI, Limitada, Individual ou Associação).")
                .When(x => DocumentoHelper.EhCnpj(x.CpfCnpj));

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("Informe o celular do titular da subconta.")
                .Matches(@"^\d{10,11}$").WithMessage("Informe o celular com DDD, ex: (11) 99999-8888.");

            RuleFor(x => x.FaturamentoMensal)
                .NotNull().WithMessage("Informe o faturamento (ou renda) mensal.")
                .GreaterThan(0m).WithMessage("O faturamento mensal deve ser maior que zero.");

            RuleFor(x => x.Endereco).NotEmpty().WithMessage("Informe o endereço.");
            RuleFor(x => x.Numero).NotEmpty().WithMessage("Informe o número do endereço.");
            RuleFor(x => x.Bairro).NotEmpty().WithMessage("Informe o bairro.");
            RuleFor(x => x.Cep)
                .NotEmpty().WithMessage("Informe o CEP.")
                .Matches(@"^\d{8}$").WithMessage("O CEP deve ter 8 dígitos.");
        }
    }
}
