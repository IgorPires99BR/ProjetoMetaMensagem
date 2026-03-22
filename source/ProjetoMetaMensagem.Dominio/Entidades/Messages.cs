using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Messages
    {
        public int id { get; set; }
        public string? company_id { get; set; }
        public string? phone { get; set; }
        public string? direction { get; set; }
        public string? text { get; set; }
        public DateTimeOffset created_at { get; set; }
    }
}
