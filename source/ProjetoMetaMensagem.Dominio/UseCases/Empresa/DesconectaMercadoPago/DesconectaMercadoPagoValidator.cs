using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DesconectaMercadoPago
{
    public class DesconectaMercadoPagoValidator : AbstractValidator<DesconectaMercadoPagoCommand>
    {
        public DesconectaMercadoPagoValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
