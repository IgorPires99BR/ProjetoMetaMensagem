using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida
{
    public class ListaMensagemRecebidaValidator : AbstractValidator<ListaMensagemRecebidaCommand>
    {
        public ListaMensagemRecebidaValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa para listar as mensagens.");

            RuleFor(x => x.ContatoId)
                .NotEmpty().WithMessage("Não foi possível identificar o contato da conversa.");

            RuleFor(x => x.Pagina)
                .GreaterThanOrEqualTo(0).WithMessage("A página não pode ser negativa.");

            // Teto para o infinite scroll nao conseguir puxar o historico inteiro em uma unica chamada.
            RuleFor(x => x.TamanhoPagina)
                .InclusiveBetween(1, 200).WithMessage("O tamanho da página deve ficar entre 1 e 200.");
        }
    }
}
