using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaValidator : AbstractValidator<AtivaCoexistenciaCommand>
    {
        public AtivaCoexistenciaValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");
            RuleFor(x => x.IdEmpresa).NotEmpty().WithMessage("A empresa é obrigatória.");
        }
    }
}
