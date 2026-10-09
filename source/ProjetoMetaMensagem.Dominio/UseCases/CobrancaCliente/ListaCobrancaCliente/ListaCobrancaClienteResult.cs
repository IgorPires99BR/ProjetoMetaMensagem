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
        public CobrancaClienteResumo(Entidades.CobrancaCliente cobranca, string? nomeCliente, string? nomeContato, string? telefone)
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
            // Baixa feita pelo Itau sempre grava o EndToEndId; a manual nao.
            PagoViaPix = cobranca.EndToEndId != null;
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
