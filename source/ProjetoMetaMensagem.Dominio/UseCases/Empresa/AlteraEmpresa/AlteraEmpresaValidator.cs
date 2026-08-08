using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa
{
    public class AlteraEmpresaValidator : AbstractValidator<AlteraEmpresaCommand>
    {
        public AlteraEmpresaValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa que será alterada.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Informe o nome da empresa.")
                .MaximumLength(255).WithMessage("O nome da empresa deve ter no máximo 255 caracteres.");

            // Diferente do cadastro, aqui o CNPJ nao e obrigatorio: existem empresas antigas
            // gravadas sem CNPJ e exigir o campo impediria qualquer edicao desses registros.
            RuleFor(x => x.Cnpj)
                .MaximumLength(20).WithMessage("O CNPJ deve ter no máximo 20 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(255).WithMessage("O e-mail deve ter no máximo 255 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Telefone)
                .MaximumLength(50).WithMessage("O telefone deve ter no máximo 50 caracteres.");

            RuleFor(x => x.WabaId)
                .MaximumLength(100).WithMessage("O WABA ID deve ter no máximo 100 caracteres.");

            RuleFor(x => x.PhoneNumberId)
                .MaximumLength(100).WithMessage("O Phone Number ID deve ter no máximo 100 caracteres.");

            RuleFor(x => x.AppIdMeta)
                .MaximumLength(100).WithMessage("O App ID da Meta deve ter no máximo 100 caracteres.");

            RuleFor(x => x.PlanoId)
                .MaximumLength(50).WithMessage("O plano deve ter no máximo 50 caracteres.");
        }
    }
}
