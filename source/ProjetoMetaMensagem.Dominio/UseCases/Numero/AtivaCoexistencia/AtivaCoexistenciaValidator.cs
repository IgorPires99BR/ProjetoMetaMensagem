using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaValidator : AbstractValidator<AtivaCoexistenciaCommand>
    {
        public AtivaCoexistenciaValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");
            RuleFor(x => x.IdEmpresa).NotEmpty().WithMessage("A empresa é obrigatória.");
            RuleFor(x => x.Pin)
                .NotEmpty().WithMessage("O PIN de verificação em 2 etapas do número é obrigatório.")
                .Matches("^[0-9]{6}$").WithMessage("O PIN deve ter exatamente 6 dígitos.");
        }
    }
}
