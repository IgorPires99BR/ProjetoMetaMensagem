using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoCommand : IRequest<Response<AlteraContatoResult>>
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? NomeContato { get; set; }
        public string? Email { get; set; }

        public string? NomeCliente { get; set; }
        public int? DiaVencimento { get; set; }
        public decimal? TaxaJuros { get; set; }
        public decimal? TaxaJurosMensal { get; set; }
        public decimal? ValorFatura { get; set; }

        public Guid EmpresaId { get; set; }

        public DateTimeOffset DataCriacao { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem isso o UPDATE
        // casava so pelo Id e permitia alterar contato de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
