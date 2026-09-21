using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Contato
    {
        // Gera o Id aqui (mesmo padrao do HistoricoDisparo) porque o INSERT grava a coluna Id:
        // a importacao em lote monta o contato por este construtor e, sem isso, todos sairiam
        // com Guid.Empty e colidiriam na chave primaria a partir do segundo.
        // Ao ler do banco nao atrapalha: o Dapper sobrescreve com o valor da linha.
        public Contato()
        {
            Id = Guid.NewGuid();
        }

        // Padrao de negocio atual (2026): quem preenche os dados financeiros do contato e
        // nao informa um valor especifico cai nesses defaults -- mesmos valores que estavam
        // na entidade Cliente removida (ver historico: unificada aqui a pedido do Igor).
        public const int DiaVencimentoPadrao = 20;
        public const decimal TaxaJurosPadrao = 2.00m;
        public const decimal TaxaJurosMensalPadrao = 2.00m;
        public const decimal ValorFaturaPadrao = 405.00m;

        public Contato(CriaContatoCommand command)
        {
            Id = Guid.NewGuid();
            UsuarioId = command.UsuarioId;
            EmpresaId = command.EmpresaId;
            Telefone = command.Telefone;
            NomeContato = command.NomeContato;
            Email = command.Email;
            NomeCliente = command.NomeCliente;
            DiaVencimento = command.DiaVencimento ?? DiaVencimentoPadrao;
            TaxaJuros = command.TaxaJuros ?? TaxaJurosPadrao;
            TaxaJurosMensal = command.TaxaJurosMensal ?? TaxaJurosMensalPadrao;
            ValorFatura = command.ValorFatura ?? ValorFaturaPadrao;
            DataCriacao = DateTime.Now;
        }

        public Contato(AlteraContatoCommand command)
        {
            Id = command.Id;
            UsuarioId = command.UsuarioId;
            EmpresaId = command.EmpresaId;
            Telefone = command.Telefone;
            NomeContato = command.NomeContato;
            Email = command.Email;
            NomeCliente = command.NomeCliente;
            DiaVencimento = command.DiaVencimento ?? DiaVencimentoPadrao;
            TaxaJuros = command.TaxaJuros ?? TaxaJurosPadrao;
            TaxaJurosMensal = command.TaxaJurosMensal ?? TaxaJurosMensalPadrao;
            ValorFatura = command.ValorFatura ?? ValorFaturaPadrao;
            DataCriacao = DateTime.Now;
        }
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // Vinculo direto com a empresa (alem de UsuarioId, que continua sendo "quem cadastrou").
        // Antes o escopo por empresa so existia via JOIN em Usuario; agora e coluna propria.
        [Required]
        public Guid EmpresaId { get; set; }

        [Required] // Único obrigatório conforme regra de negócio
        [MaxLength(50)]
        public string Telefone { get; set; }

        // Nome de quem atende esse numero de WhatsApp (pode ser diferente do titular da fatura).
        [MaxLength(255)]
        public string? NomeContato { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        // Nome de quem deve a fatura (o "cliente" no sentido financeiro) -- so preenchido por
        // quem usa esse conceito (ex: Sebrecon). Pode ser o mesmo nome de NomeContato ou nao.
        [MaxLength(255)]
        public string? NomeCliente { get; set; }

        public int? DiaVencimento { get; set; }

        public decimal? TaxaJuros { get; set; }

        public decimal? TaxaJurosMensal { get; set; }

        public decimal? ValorFatura { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
