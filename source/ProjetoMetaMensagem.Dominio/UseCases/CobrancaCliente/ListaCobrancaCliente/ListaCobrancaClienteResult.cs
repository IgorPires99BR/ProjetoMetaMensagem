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
        }

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
