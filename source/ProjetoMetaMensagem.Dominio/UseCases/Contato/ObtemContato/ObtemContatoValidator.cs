using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoValidator : AbstractValidator<ObtemContatoCommand>
    {
        public ObtemContatoValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário logado para listar os contatos.");
        }
    }
}
