using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.AssumirConversa
{
    public class AssumirConversaCommand : IRequest<Response<AssumirConversaResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }

        // Preenchido pelo controller a partir do JWT, nunca pelo corpo: e quem fica registrado
        // como dono da conversa.
        public Guid UsuarioId { get; set; }
    }
}
