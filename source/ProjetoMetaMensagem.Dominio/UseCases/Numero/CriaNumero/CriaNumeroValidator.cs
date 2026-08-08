using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero
{
    public class CriaNumeroValidator : AbstractValidator<CriaNumeroCommand>
    {
        public CriaNumeroValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário responsável pelo número.");

            // Sem a empresa o handler nao consegue recuperar o WABA ID nem o token da Meta.
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa do número.");

            RuleFor(x => x.NumeroTelefone)
                .NotEmpty().WithMessage("Informe o número de telefone.")
                .MaximumLength(50).WithMessage("O número de telefone deve ter no máximo 50 caracteres.");

            // O nome verificado e gravado na coluna Descricao do Numero.
            RuleFor(x => x.NomeEmpresa)
                .MaximumLength(100).WithMessage("O nome verificado deve ter no máximo 100 caracteres.");
        }
    }
}
