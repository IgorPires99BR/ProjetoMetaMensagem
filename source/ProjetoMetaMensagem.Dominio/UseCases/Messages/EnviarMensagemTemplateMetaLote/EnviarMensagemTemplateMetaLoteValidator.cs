using FluentValidation;
using System;
using System.Collections.Generic;
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
            // So se aplica quando NAO ha personalizacao por telefone: com "Nome do contato"/"Telefone
            // do contato" numa variavel, o front deixa esse slot vazio de proposito no ParametrosBody
            // "global" (o valor real vai em ParametrosBodyPorTelefone) -- sem o When() essa regra
            // barrava todo disparo personalizado mesmo com os valores efetivos completos.
            RuleForEach(x => x.ParametrosBody)
                .NotEmpty().WithMessage("Preencha todas as variáveis do template antes de disparar.")
                .When(x => !x.TemValoresPorDestinatario);

            // Com personalizacao por destinatario, os valores que valem sao os de cada telefone --
            // um contato sem nome cadastrado deixaria a variavel vazia e a Meta recusaria so aquele
            // envio, no meio do lote.
            RuleFor(x => x)
                .Must(command => Enumerable.Range(0, (command.Telefones ?? new List<string>()).Count)
                    .All(i => command.ParametrosBodyDoDestinatario(i).All(valor => !string.IsNullOrWhiteSpace(valor))))
                .WithMessage("Há contato selecionado sem valor para alguma variável da mensagem. Preencha um valor fixo ou tire esse contato da lista.")
                .When(x => x.TemValoresPorDestinatario);

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
