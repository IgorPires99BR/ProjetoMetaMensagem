using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Conversations
    {
        public Guid id { get; set; }
        public string? company_id { get; set; }
        public string? phone { get; set; }
        public string? status_funil { get; set; }
        public string? status { get; set; }
        public Guid? step { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public DateTimeOffset updated_at { get; set; }
    }
}
