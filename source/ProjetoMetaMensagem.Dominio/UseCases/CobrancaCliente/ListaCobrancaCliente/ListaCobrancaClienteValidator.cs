using FluentValidation;
using ProjetoMetaMensagem.Dominio.Entidades;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.ListaCobrancaCliente
{
    public class ListaCobrancaClienteValidator : AbstractValidator<ListaCobrancaClienteCommand>
    {
        private static readonly string[] StatusValidos =
        {
            StatusCobrancaCliente.Pendente, StatusCobrancaCliente.Paga,
            StatusCobrancaCliente.Vencida, StatusCobrancaCliente.Cancelada
        };

        public ListaCobrancaClienteValidator()
        {
            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || StatusValidos.Contains(status))
                .WithMessage("Status inválido. Use PENDENTE, PAGA, VENCIDA ou CANCELADA.");

            RuleFor(x => x)
                .Must(x => !x.DataInicio.HasValue || !x.DataFim.HasValue || x.DataInicio.Value.Date <= x.DataFim.Value.Date)
                .WithMessage("A data inicial não pode ser maior que a data final.");
        }
    }
}
