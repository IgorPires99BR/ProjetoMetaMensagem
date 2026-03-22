using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Flows
    {
        public string id { get; set; } = null!;
        public string? company_id { get; set; }
        public string name { get; set; } = null!;
        public string? messages { get; set; } // Mapeia o JSON do banco como string
        public DateTimeOffset updated_at { get; set; }
    }
}
