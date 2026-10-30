using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Status da cobranca que quem revende a plataforma (ex: Sebrecon) faz ao PROPRIO cliente
    // dela (ex: um devedor do Gilson). Nao confundir com StatusAssinatura (Assinatura.cs), que
    // e a cobranca da assinatura SaaS da propria Contact Solution.
    public static class StatusCobrancaCliente
    {
        public const string Pendente = "PENDENTE";
        public const string Paga = "PAGA";
        public const string Vencida = "VENCIDA";
        public const string Cancelada = "CANCELADA";
    }

    // Uma linha por disparo de Template com GeraCobranca = true: a cobranca do cliente final de
    // quem revende a plataforma, aberta no momento do envio (ver CobrancaClienteFactory).
    //
    // Valor/DataVencimento sao snapshot do Contato no disparo -- nao mudam se o cadastro do
    // contato for alterado depois, porque a cobranca ja foi enviada com aqueles valores.
    public class CobrancaCliente
    {
        public CobrancaCliente()
        {
            Id = Guid.NewGuid();
            Status = StatusCobrancaCliente.Pendente;
            DataCriacao = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid HistoricoDisparoId { get; set; }

        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }

        public string Status { get; set; } = StatusCobrancaCliente.Pendente;
        public DateTime? DataPagamento { get; set; }

        // Chave de correlacao com o webhook da Cakto (utm_content do link de pagamento) -- sem
        // uso ainda, porque o link hoje e o mesmo checkout compartilhado da Contact Solution.
        // Confirmacao de pagamento por enquanto e manual (ver MarcaCobrancaClientePaga).
        public string? UtmContentCakto { get; set; }
        public string? EventoIdCakto { get; set; }

        // Pix gerado na conta da plataforma no provedor ativo (ver BD/52, BD/54 e BD/58). Nulos
        // quando a empresa nao tem cobranca Pix ativa -- a cobranca continua existindo, so sem
        // baixa automatica. Txid e nosso (externalReference no Asaas, external_reference no
        // Mercado Pago); PixIdExterno e o id da cobranca no Asaas ou da order no Mercado Pago.
        public string? Txid { get; set; }
        public string? PixIdExterno { get; set; }
        public string? PixCopiaECola { get; set; }
        public DateTime? PixExpiraEm { get; set; }
        public decimal? Multa { get; set; }
        public decimal? Juros { get; set; }
        public decimal? ValorCobrado { get; set; }
        public decimal? ValorPago { get; set; }
        public string? EndToEndId { get; set; }

        // Split do ValorCobrado no momento do disparo (ver BD/53): ValorComissao fica com a
        // plataforma, ValorRepasse vai para a subconta Asaas da empresa. Snapshot -- mudar a comissao
        // depois nao altera cobranca ja enviada.
        public decimal? ValorComissao { get; set; }
        public decimal? ValorRepasse { get; set; }

        // Pix gerado sem split, com o valor inteiro na conta da plataforma (ver BD/57): comissao e
        // repasse ficam nulos e o acerto com a empresa e feito por fora. Snapshot do disparo.
        public bool RecebidaNaPlataforma { get; set; }

        // Quem gerou o Pix (ProvedorCobranca, ver BD/58): a baixa e o cancelamento voltam nele
        // mesmo depois de o provedor ativo mudar. Nulo = Asaas (cobrancas anteriores a coluna).
        public string? Provedor { get; set; }

        // Pagina de pagamento do proprio provedor (ticket_url do Mercado Pago). O link enviado ao
        // cliente continua sendo a nossa pagina /pix/{txid}, que aponta para esta.
        public string? LinkPagamentoExterno { get; set; }

        // Txid no formato aceito pelo Bacen (26 a 35 alfanumericos): o proprio Id sem hifens.
        public static string TxidDe(Guid id) => id.ToString("N");

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
