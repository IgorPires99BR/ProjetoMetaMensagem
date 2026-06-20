using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows
{
    public class ListaFlowsCommand : IRequest<Response<List<ListaFlowsResult>>>
    {
        public ListaFlowsCommand(Guid idEmpresa)
        {
            IdEmpresa = idEmpresa;
        }
        public Guid IdEmpresa { get; set; }
    }
}
