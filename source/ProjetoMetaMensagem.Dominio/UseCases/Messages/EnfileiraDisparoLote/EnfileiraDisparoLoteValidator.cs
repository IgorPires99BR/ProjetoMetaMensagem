using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnfileiraDisparoLote
{
    public class EnfileiraDisparoLoteValidator : AbstractValidator<EnfileiraDisparoLoteCommand>
    {
        public EnfileiraDisparoLoteValidator()
        {
            RuleFor(x => x.IdEmpresa).NotEmpty().WithMessage("Não foi possível identificar a empresa responsável pelo disparo.");
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Selecione o modelo que será enviado.");
            RuleFor(x => x.ContatosIds).NotEmpty().WithMessage("Selecione ao menos um contato para o disparo.");
            RuleFor(x => x.ChaveIdempotencia).NotEmpty().WithMessage("Recarregue a página e tente novamente.");

            RuleFor(x => x.ParametroHeaderMediaUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Informe um link válido para a mídia do cabeçalho.")
                .When(x => !string.IsNullOrEmpty(x.ParametroHeaderMediaUrl));
        }
    }
}
