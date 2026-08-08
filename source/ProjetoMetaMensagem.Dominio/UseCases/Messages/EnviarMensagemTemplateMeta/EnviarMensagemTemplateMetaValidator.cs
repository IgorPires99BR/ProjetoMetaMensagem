using FluentValidation;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaValidator : AbstractValidator<EnviarMensagemTemplateMetaCommand>
    {
        public EnviarMensagemTemplateMetaValidator()
        {
            // Regras propositalmente enxutas: alem do disparo manual, este command tambem e montado
            // pelo CampanhaWorker, que preenche apenas IdEmpresa, ContatoId, Telefone e TemplateId.
            // Exigir NomeTemplate aqui derrubaria o processamento de campanhas.
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa responsável pelo disparo.");

            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("Informe o telefone de destino da mensagem.");

            // Lista vazia passa; a regra so barra variavel declarada e deixada em branco,
            // que a Meta rejeita com erro cru no meio do disparo.
            RuleForEach(x => x.ParametrosBody)
                .NotEmpty().WithMessage("Preencha todas as variáveis do template antes de disparar.");

            RuleFor(x => x.ParametroHeaderMediaUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Informe um link válido para a mídia do cabeçalho.")
                .When(x => !string.IsNullOrEmpty(x.ParametroHeaderMediaUrl));
        }
    }
}
