using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas
{
    public class ListaConversaPorIdCommand : IRequest<Response<List<ListaConversaPorIdResult>>>
    {
        public string CompanyId{ get; set; }
    }
}
