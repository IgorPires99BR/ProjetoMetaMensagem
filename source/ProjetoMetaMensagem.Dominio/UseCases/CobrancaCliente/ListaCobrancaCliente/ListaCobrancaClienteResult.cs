using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.ListaCobrancaCliente
{
    public class ListaCobrancaClienteResult
    {
        public List<CobrancaClienteResumo> Cobrancas { get; set; } = new();
    }

    public class CobrancaClienteResumo
    {
        public CobrancaClienteResumo(Entidades.CobrancaCliente cobranca, string? nomeCliente, string? nomeContato, string? telefone,
            string? linkPagamento = null)
        {
            Id = cobranca.Id;
            EmpresaId = cobranca.EmpresaId;
            ContatoId = cobranca.ContatoId;
            TemplateId = cobranca.TemplateId;
            HistoricoDisparoId = cobranca.HistoricoDisparoId;
            Valor = cobranca.Valor;
            DataVencimento = cobranca.DataVencimento;
            Status = cobranca.Status;
            DataPagamento = cobranca.DataPagamento;
            DataCriacao = cobranca.DataCriacao;
            NomeCliente = nomeCliente;
            NomeContato = nomeContato;
            Telefone = telefone;
            TemPix = cobranca.Txid != null;
            ValorCobrado = cobranca.ValorCobrado;
            Multa = cobranca.Multa;
            Juros = cobranca.Juros;
            ValorPago = cobranca.ValorPago;
            PixExpiraEm = cobranca.PixExpiraEm;
            // Baixa automatica sempre grava o ValorPago (vindo do provedor); a manual nao. O
            // EndToEndId nao serve: o Asaas nao o devolve na consulta da cobranca.
            PagoViaPix = cobranca.ValorPago != null;
            ValorComissao = cobranca.ValorComissao;
            ValorRepasse = cobranca.ValorRepasse;
            RecebidaNaPlataforma = cobranca.RecebidaNaPlataforma;
            Provedor = cobranca.Txid != null ? cobranca.Provedor ?? Interfaces.Servicos.ProvedorCobranca.Asaas : null;
            LinkPagamento = cobranca.Txid != null ? linkPagamento : null;
        }

        // Pix (ver BD/52). O copia e cola fica de fora: a tela nao precisa dele e a lista
        // inteira de codigos pagaveis nao tem por que trafegar.
        public bool TemPix { get; set; }
        public decimal? ValorCobrado { get; set; }
        public decimal? Multa { get; set; }
        public decimal? Juros { get; set; }
        public decimal? ValorPago { get; set; }
        public DateTime? PixExpiraEm { get; set; }
        public bool PagoViaPix { get; set; }

        // Split do Pix: o que fica com a plataforma e o que vai para a subconta da empresa.
        public decimal? ValorComissao { get; set; }
        public decimal? ValorRepasse { get; set; }

        // Sem split: o valor inteiro caiu na conta da Contact Solution (acerto feito por fora).
        public bool RecebidaNaPlataforma { get; set; }

        // Quem gerou o Pix (ASAAS ou MERCADOPAGO); nulo sem Pix.
        public string? Provedor { get; set; }

        // Mesma pagina enviada ao cliente no botao do WhatsApp: a empresa pode reenviar o link
        // ou conferir o que o cliente esta vendo. So mostra o que o cliente ja recebeu.
        public string? LinkPagamento { get; set; }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid HistoricoDisparoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string Status { get; set; }
        public DateTime? DataPagamento { get; set; }
        public DateTime DataCriacao { get; set; }

        // Dados do Contato pra exibir na tela sem consulta extra do front.
        public string? NomeCliente { get; set; }
        public string? NomeContato { get; set; }
        public string? Telefone { get; set; }
    }
}
