using FluentValidation;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraFotoNumero
{
    public class AlteraFotoNumeroValidator : AbstractValidator<AlteraFotoNumeroCommand>
    {
        public const int TamanhoMaximoBytes = 5 * 1024 * 1024;
        private static readonly string[] TiposAceitos = { "image/jpeg", "image/png" };

        public AlteraFotoNumeroValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");

            RuleFor(x => x.Arquivo)
                .NotEmpty().WithMessage("Envie a imagem da foto.")
                .Must(a => a == null || a.Length <= TamanhoMaximoBytes).WithMessage("A foto pode ter no máximo 5 MB.");

            RuleFor(x => x.MimeType)
                .Must(t => TiposAceitos.Contains(t?.ToLowerInvariant()))
                .WithMessage("A foto precisa ser JPG ou PNG.");
        }
    }
}
