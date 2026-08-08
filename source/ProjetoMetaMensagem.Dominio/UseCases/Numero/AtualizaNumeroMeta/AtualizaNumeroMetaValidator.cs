using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta
{
    public class AtualizaNumeroMetaValidator : AbstractValidator<AtualizaNumeroMetaCommand>
    {
        public AtualizaNumeroMetaValidator()
        {
            RuleFor(x => x.IdUsuario)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário logado para sincronizar os números.");

            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa para sincronizar os números com a Meta.");
        }
    }
}
