using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote
{
    public class CriaContatoEmLoteCommand : IRequest<Response<CriaContatoEmLoteResult>>
    {
        public Guid IdEmpresa { get; set; }
        public Guid UsuarioId { get; set; }
        public List<ContatoItemDto> Contatos { get; set; } = new();
    }

    public class ContatoItemDto
    {
        public string? Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }
}