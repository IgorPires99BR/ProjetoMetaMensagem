using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoCommand : IRequest<Response<CriaContatoResult>>
    {
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? NomeContato { get; set; }
        public string? Email { get; set; }

        // Dados financeiros (ver Contato.DiaVencimentoPadrao/TaxaJurosPadrao/etc): nulo aqui
        // vira o padrao de negocio no Handler, nao no default do parametro, pra o front
        // poder mandar explicitamente um valor diferente sem mexer no Command.
        public string? NomeCliente { get; set; }
        public int? DiaVencimento { get; set; }
        public decimal? TaxaJuros { get; set; }
        public decimal? TaxaJurosMensal { get; set; }
        public decimal? ValorFatura { get; set; }

        // Escopo vem do token (EmpresaAccessFilter exige que bata) -- nunca aceitar de uma
        // empresa diferente da de quem esta logado (exceto conta de plataforma).
        public Guid EmpresaId { get; set; }

        public DateTimeOffset DataCriacao { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem isso, o
        // UsuarioId do corpo definia sozinho a empresa do contato (via Usuario.EmpresaId) e o
        // EmpresaAccessFilter nao tem como saber que "UsuarioId" e um id de empresa por tabela:
        // um usuario comum de outra empresa injetava contato na empresa alheia mandando o id de
        // um usuario de la.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
