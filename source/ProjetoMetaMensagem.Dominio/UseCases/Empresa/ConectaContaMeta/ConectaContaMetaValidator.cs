using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConectaContaMeta
{
    public class ConectaContaMetaValidator : AbstractValidator<ConectaContaMetaCommand>
    {
        public ConectaContaMetaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Código de autorização da Meta não informado.");

            RuleFor(x => x.PhoneNumberId)
                .NotEmpty().WithMessage("A Meta não retornou o identificador do número (phone_number_id).");

            RuleFor(x => x.SolicitanteEhAdminDaPlataforma)
                .Equal(true).WithMessage("Apenas a conta de plataforma pode conectar uma empresa direto à Meta.");
        }
    }
}
