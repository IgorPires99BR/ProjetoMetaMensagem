using FluentValidation;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteValidator : AbstractValidator<EnviarMensagemTemplateMetaLoteCommand>
    {
        public EnviarMensagemTemplateMetaLoteValidator()
        {
            // Sem a empresa nao ha PhoneNumberId nem token da Meta para o disparo.
            RuleFor(x => x.IdEmpresa)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa responsável pelo disparo.");

            RuleFor(x => x.Telefones)
                .NotEmpty().WithMessage("Selecione ao menos um contato para o disparo.");

            RuleFor(x => x.NomeTemplate)
                .NotEmpty().WithMessage("Selecione o template que será disparado.");

            RuleFor(x => x.Idioma)
                .NotEmpty().WithMessage("Informe o idioma do template.");

            RuleForEach(x => x.Telefones)
                .NotEmpty().WithMessage("Há um contato selecionado sem telefone cadastrado.");

            // Os parametros do corpo sao enviados a Meta sem nenhum filtro (diferente dos de botao,
            // que descartam os vazios), entao uma variavel em branco vira erro cru da Meta no disparo.
            RuleForEach(x => x.ParametrosBody)
                .NotEmpty().WithMessage("Preencha todas as variáveis do template antes de disparar.");

            // Telefones e ContatosIds sao percorridos em paralelo pelo mesmo indice. Se as listas
            // tiverem tamanhos diferentes, o historico do disparo e gravado com contato zerado.
            RuleFor(x => x.ContatosIds)
                .Must((command, contatosIds) => contatosIds.Count == command.Telefones.Count)
                .WithMessage("A lista de contatos selecionados está inconsistente. Refaça a seleção e tente novamente.")
                .When(x => x.ContatosIds != null && x.ContatosIds.Any());

            RuleFor(x => x.ParametroHeaderMediaUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Informe um link válido para a mídia do cabeçalho.")
                .When(x => !string.IsNullOrEmpty(x.ParametroHeaderMediaUrl));
        }
    }
}
