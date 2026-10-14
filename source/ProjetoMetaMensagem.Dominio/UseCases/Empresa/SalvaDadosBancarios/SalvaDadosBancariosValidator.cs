using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    public class SalvaDadosBancariosValidator : AbstractValidator<SalvaDadosBancariosCommand>
    {
        public SalvaDadosBancariosValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            // Comissao so e lida do comando quando quem salva e admin da plataforma (ver handler).
            When(x => x.SolicitanteEhAdminDaPlataforma && (x.ComissaoTipo != null || x.ComissaoValor.HasValue), () =>
            {
                RuleFor(x => x.ComissaoTipo)
                    .Must(t => t == DadosBancariosEmpresa.ComissaoPercentual || t == DadosBancariosEmpresa.ComissaoFixa)
                    .WithMessage("Escolha se a comissão é percentual ou valor fixo.");

                RuleFor(x => x.ComissaoValor)
                    .NotNull().WithMessage("Informe o valor da comissão.")
                    .GreaterThanOrEqualTo(0m).WithMessage("A comissão não pode ser negativa.");

                // 100% ou mais nao deixaria nada para repassar a empresa.
                RuleFor(x => x.ComissaoValor)
                    .LessThan(100m).WithMessage("A comissão percentual deve ser menor que 100%.")
                    .When(x => x.ComissaoTipo == DadosBancariosEmpresa.ComissaoPercentual);
            });
        }
    }
}
