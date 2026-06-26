using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato
{
    public class AssociarTagsContatoCommand : IRequest<Response<AssociarTagsContatoResult>>
    {
        public Guid ContatoId { get; set; }
        public List<Guid> TagIds { get; set; }
    }
}
