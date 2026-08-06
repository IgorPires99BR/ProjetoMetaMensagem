using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia
{
    public class ObtemMidiaCommand : IRequest<Response<ObtemMidiaResult>>
    {
        public string MidiaId { get; set; }
        public Guid EmpresaId { get; set; }
    }
}
