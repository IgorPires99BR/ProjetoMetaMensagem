using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros
{
    public class ListarNumerosValidator : AbstractValidator<ListarNumerosCommand>
    {
        public ListarNumerosValidator()
        {
            RuleFor(x => x.IdUsuario)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário logado para listar os números.");
        }
    }
}
