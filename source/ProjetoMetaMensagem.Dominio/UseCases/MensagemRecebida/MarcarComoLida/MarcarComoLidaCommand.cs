using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida
{
    public class MarcarComoLidaCommand : IRequest<Response<MarcarComoLidaResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
