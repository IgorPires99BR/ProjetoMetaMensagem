using FluentValidation;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraPerfilNumero
{
    public class AlteraPerfilNumeroValidator : AbstractValidator<AlteraPerfilNumeroCommand>
    {
        // Lista fechada da Meta (campo vertical do whatsapp_business_profile).
        public static readonly string[] SegmentosValidos =
        {
            "ALCOHOL", "APPAREL", "AUTO", "BEAUTY", "EDU", "ENTERTAIN", "EVENT_PLAN", "FINANCE", "GOVT",
            "GROCERY", "HEALTH", "HOTEL", "NONPROFIT", "ONLINE_GAMBLING", "OTC_DRUGS", "OTHER",
            "PHYSICAL_GAMBLING", "PROF_SERVICES", "RESTAURANT", "RETAIL", "TRAVEL"
        };

        public AlteraPerfilNumeroValidator()
        {
            RuleFor(x => x.NumeroId).NotEmpty().WithMessage("O número é obrigatório.");

            // Limites da Meta -- validados aqui pra mensagem sair em português e apontar o campo.
            RuleFor(x => x.Sobre).MaximumLength(139).WithMessage("O recado pode ter no máximo 139 caracteres.");
            RuleFor(x => x.Descricao).MaximumLength(512).WithMessage("A descrição pode ter no máximo 512 caracteres.");
            RuleFor(x => x.Endereco).MaximumLength(256).WithMessage("O endereço pode ter no máximo 256 caracteres.");
            RuleFor(x => x.Email)
                .MaximumLength(128).WithMessage("O e-mail pode ter no máximo 128 caracteres.")
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("Informe um e-mail válido.");

            RuleFor(x => x.Sites)
                .Must(s => s == null || s.Count(x => !string.IsNullOrWhiteSpace(x)) <= 2)
                .WithMessage("A Meta aceita no máximo 2 sites.");
            RuleForEach(x => x.Sites)
                .Must(s => string.IsNullOrWhiteSpace(s) || (s.Trim().Length <= 256
                    && (s.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase))))
                .WithMessage("Cada site precisa começar com http:// ou https:// e ter no máximo 256 caracteres.");

            RuleFor(x => x.Segmento)
                .Must(s => string.IsNullOrEmpty(s) || SegmentosValidos.Contains(s))
                .WithMessage("Segmento inválido.");
        }
    }
}
