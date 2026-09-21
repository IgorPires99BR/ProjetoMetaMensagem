using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoResult
    {
        public ObtemContatoResult(Entidades.Contato contato, Entidades.OrigemLead? origem = null)
        {
            Id = contato.Id;
            UsuarioId = contato.UsuarioId;
            EmpresaId = contato.EmpresaId;
            Telefone = contato.Telefone;
            NomeContato = contato.NomeContato;
            Email = contato.Email;
            NomeCliente = contato.NomeCliente;
            DiaVencimento = contato.DiaVencimento;
            TaxaJuros = contato.TaxaJuros;
            TaxaJurosMensal = contato.TaxaJurosMensal;
            ValorFatura = contato.ValorFatura;
            DataCriacao = contato.DataCriacao;

            // Origem gravada na primeira mensagem de quem chegou por um anuncio Click-to-
            // WhatsApp (OrigemLead), mas nunca lida fora do momento da compra -- ate agora
            // ninguem via de qual anuncio um lead vinha. Null pra quem foi cadastrado
            // manualmente ou escreveu organicamente, sem passar por anuncio nenhum.
            OrigemAnuncio = origem?.Headline ?? origem?.SourceId;
            OrigemData = origem?.DataPrimeiroContato;
        }
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid EmpresaId { get; set; }
        public string Telefone { get; set; }
        public string? NomeContato { get; set; }
        public string? Email { get; set; }
        public string? NomeCliente { get; set; }
        public int? DiaVencimento { get; set; }
        public decimal? TaxaJuros { get; set; }
        public decimal? TaxaJurosMensal { get; set; }
        public decimal? ValorFatura { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? OrigemAnuncio { get; set; }
        public DateTime? OrigemData { get; set; }
    }
}
