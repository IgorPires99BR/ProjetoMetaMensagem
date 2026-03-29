using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Flows
    {
        public Flows()
        {
            
        }

        public Flows(CriaFlowCommand command)
        {
            id = command.Id;
            company_id = command.CompanyId;
            name = command.Name;
            messages = command.Messages;
            updated_at = DateTime.Now;
        }

        public string id { get; set; } = null!;
        public string? company_id { get; set; }
        public string name { get; set; } = null!;
        public string? messages { get; set; } // Mapeia o JSON do banco como string
        public DateTimeOffset updated_at { get; set; }
    }
}
