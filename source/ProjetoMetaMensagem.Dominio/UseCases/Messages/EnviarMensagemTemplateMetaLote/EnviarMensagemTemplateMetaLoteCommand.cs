using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteCommand : IRequest<Response<EnviarMensagemTemplateMetaLoteResult>>
    {
        public Guid IdEmpresa { get; set; }
        public List<string> ContatosIds { get; set; }
        public List<string> Telefones { get; set; } = new List<string>();
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; } = "pt_BR";
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid? TemplateId { get; set; }

        // ADICIONE ESTE CAMPO PARA SUPORTAR MÍDIAS NO HEADER DURANTE O ENVIO EM LOTE
        public string? ParametroHeaderMediaUrl { get; set; }
        public List<string> ParametrosBody { get; set; } = new List<string>();
        public List<string> ParametrosButton { get; set; } = new List<string>();

        // Valores das variáveis por destinatário, indexados pelo telefone como veio na lista.
        // Sem isto o lote inteiro ia com os MESMOS valores: um template que começa com
        // "Olá {{1}}" mandava o mesmo nome para os 500 contatos. Quem não informar continua
        // caindo em ParametrosBody, que vale para todo mundo.
        public Dictionary<string, List<string>> ParametrosBodyPorTelefone { get; set; } = new Dictionary<string, List<string>>();

        // Definicao de cada variavel do corpo (campo do contato, parametro cadastrado ou texto
        // fixo). Quando informada, o handler resolve os valores POR CONTATO no servidor e
        // sobrescreve ParametrosBody/ParametrosBodyPorTelefone -- assim campos como valorFatura
        // e dataVencimento nao dependem de o front conhecer todos os dados do contato.
        public List<Entidades.AgendamentoVariavelDto> Variaveis { get; set; } = new List<Entidades.AgendamentoVariavelDto>();

        // Mesma ideia, mas indexada pelo Id do contato. E a que vale quando a lista tem contatos
        // que compartilham o mesmo telefone (uma pessoa que cuida de varias empresas): por
        // telefone o valor do ultimo contato sobrescrevia o dos outros e todos recebiam a fatura
        // errada. Preenchida por ResolvedorDeVariaveis; tem prioridade sobre ParametrosBodyPorTelefone.
        public Dictionary<string, List<string>> ParametrosBodyPorContato { get; set; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public bool TemValoresPorDestinatario =>
            (ParametrosBodyPorContato?.Any() ?? false) || (ParametrosBodyPorTelefone?.Any() ?? false);

        // Valores efetivos para um destinatário: os do contato, se houver; senão os do telefone
        // (legado); senão os globais.
        public List<string> ParametrosBodyDe(string telefone, string? contatoId = null)
        {
            if (!string.IsNullOrEmpty(contatoId) && ParametrosBodyPorContato != null &&
                ParametrosBodyPorContato.TryGetValue(contatoId, out var doContato) &&
                doContato != null && doContato.Count > 0)
            {
                return doContato;
            }

            if (ParametrosBodyPorTelefone != null &&
                ParametrosBodyPorTelefone.TryGetValue(telefone, out var doTelefone) &&
                doTelefone != null && doTelefone.Count > 0)
            {
                return doTelefone;
            }

            return ParametrosBody;
        }

        // Telefones e ContatosIds andam em paralelo pelo mesmo indice, entao o destinatario e
        // identificado pela posicao -- nunca so pelo telefone, que pode se repetir na lista.
        public List<string> ParametrosBodyDoDestinatario(int indice)
        {
            var contatoId = ContatosIds != null && indice < ContatosIds.Count ? ContatosIds[indice] : null;
            return ParametrosBodyDe(Telefones[indice], contatoId);
        }

        // Botao de URL e imagem do cabecalho por contato: o Pix de cada cobranca tem link e QR
        // proprios. Preenchidos pelo handler (PreparadorCobrancaPix), nunca pelo front; quem nao
        // tem entrada aqui usa ParametrosButton/ParametroHeaderMediaUrl.
        public Dictionary<string, List<string>> ParametrosButtonPorContato { get; set; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> HeaderMediaUrlPorContato { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string? ContatoDoDestinatario(int indice) =>
            ContatosIds != null && indice < ContatosIds.Count ? ContatosIds[indice] : null;

        public List<string> ParametrosButtonDoDestinatario(int indice)
        {
            var contatoId = ContatoDoDestinatario(indice);
            return contatoId != null && ParametrosButtonPorContato != null &&
                   ParametrosButtonPorContato.TryGetValue(contatoId, out var doContato)
                ? doContato
                : ParametrosButton;
        }

        public string? HeaderMediaUrlDoDestinatario(int indice)
        {
            var contatoId = ContatoDoDestinatario(indice);
            return contatoId != null && HeaderMediaUrlPorContato != null &&
                   HeaderMediaUrlPorContato.TryGetValue(contatoId, out var doContato)
                ? doContato
                : ParametroHeaderMediaUrl;
        }

        // Sempre fixado por quem monta o command (controller ou worker) -- ver OrigemDisparo.
        // Nao vem do corpo da requisicao nesta rota.
        public string Origem { get; set; } = ProjetoMetaMensagem.Dominio.Common.OrigemDisparo.DisparadorDeMensagem;
    }
}
