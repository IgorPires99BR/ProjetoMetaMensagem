using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag
{
    public class ListaTagCommand : IRequest<Response<List<ListaTagResult>>>
    {
        public ListaTagCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public Guid EmpresaId { get; set; }
    }
}
