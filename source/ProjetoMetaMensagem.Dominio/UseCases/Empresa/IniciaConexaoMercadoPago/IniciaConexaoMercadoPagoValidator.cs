using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.IniciaConexaoMercadoPago
{
    public class IniciaConexaoMercadoPagoValidator : AbstractValidator<IniciaConexaoMercadoPagoCommand>
    {
        public IniciaConexaoMercadoPagoValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("Não foi possível identificar a empresa.");
        }
    }
}
