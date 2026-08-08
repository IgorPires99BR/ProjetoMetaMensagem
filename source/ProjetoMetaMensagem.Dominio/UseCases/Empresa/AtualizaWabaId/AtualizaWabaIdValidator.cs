using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId
{
    public class AtualizaWabaIdValidator : AbstractValidator<AtualizaWabaIdCommand>
    {
        public AtualizaWabaIdValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
