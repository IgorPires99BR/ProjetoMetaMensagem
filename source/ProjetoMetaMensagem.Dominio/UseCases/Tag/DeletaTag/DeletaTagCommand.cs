using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag
{
    public class DeletaTagCommand : IRequest<Response<DeletaTagResult>>
    {
        public Guid Id { get; set; }
    }
}
