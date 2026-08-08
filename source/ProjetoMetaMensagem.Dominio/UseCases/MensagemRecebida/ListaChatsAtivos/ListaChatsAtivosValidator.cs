using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos
{
    public class ListaChatsAtivosValidator : AbstractValidator<ListaChatsAtivosCommand>
    {
        public ListaChatsAtivosValidator()
        {
            // IdContato e opcional: quando vem nulo a listagem devolve todos os chats da empresa.
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa para listar as conversas.");
        }
    }
}
