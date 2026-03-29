using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows
{
    public class ListaFlowsResult
    {
        public ListaFlowsResult(Entidades.Flows flow)
        {
            Id = flow.id;
            Nome = flow.name;
            Messages = flow.messages; 
        }
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Messages{ get; set; }
    }
}
