using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.DevolverAoBot
{
    public class DevolverAoBotCommand : IRequest<Response<DevolverAoBotResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
