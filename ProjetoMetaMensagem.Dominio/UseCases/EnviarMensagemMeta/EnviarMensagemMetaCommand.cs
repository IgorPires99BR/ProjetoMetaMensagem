using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.EnviarMensagemMeta
{
    public class EnviarMensagemMetaCommand : IRequest<Response<EnviarMensagemMetaResult>>
    {
        public EnviarMensagemMetaCommand(string celular, string template)
        {
            Celular = celular;
            Template = template;
        }
        public string Celular { get; set; }
        public string Template { get; set; }
    }
}
