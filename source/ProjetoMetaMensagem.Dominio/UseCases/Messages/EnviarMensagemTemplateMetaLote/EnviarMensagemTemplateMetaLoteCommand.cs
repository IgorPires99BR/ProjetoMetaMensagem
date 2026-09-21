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

        // Valores efetivos para um destinatário: os dele, se houver, senão os globais.
        public List<string> ParametrosBodyDe(string telefone)
        {
            if (ParametrosBodyPorTelefone != null &&
                ParametrosBodyPorTelefone.TryGetValue(telefone, out var doContato) &&
                doContato != null && doContato.Count > 0)
            {
                return doContato;
            }

            return ParametrosBody;
        }

        // Sempre fixado por quem monta o command (controller ou worker) -- ver OrigemDisparo.
        // Nao vem do corpo da requisicao nesta rota.
        public string Origem { get; set; } = ProjetoMetaMensagem.Dominio.Common.OrigemDisparo.DisparadorDeMensagem;
    }
}
