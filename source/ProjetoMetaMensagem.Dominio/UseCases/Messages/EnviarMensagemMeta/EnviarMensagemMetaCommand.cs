using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaCommand : IRequest<Response<EnviarMensagemMetaResult>>
    {
        public string Celular { get; set; }
        public string Template { get; set; }
        public string textoMensagem { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
