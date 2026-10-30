using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConcluiConexaoMercadoPago
{
    public class ConcluiConexaoMercadoPagoValidator : AbstractValidator<ConcluiConexaoMercadoPagoCommand>
    {
        public ConcluiConexaoMercadoPagoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("O Mercado Pago não devolveu a autorização.");
            RuleFor(x => x.State).NotEmpty().WithMessage("Retorno do Mercado Pago sem identificação da empresa.");
        }
    }
}
