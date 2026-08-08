using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote
{
    public class CriaContatoEmLoteValidator : AbstractValidator<CriaContatoEmLoteCommand>
    {
        public CriaContatoEmLoteValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("O Id da Empresa é obrigatório para importação em lote.");

            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("Não foi possível identificar o usuário responsável pela importação.");

            RuleFor(x => x.Contatos)
                .NotEmpty().WithMessage("A lista de contatos não pode estar vazia.");
        }
    }
}
