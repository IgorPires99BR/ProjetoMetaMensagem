using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Companies
    {

        public Companies()
        {
            
        }


        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public string? email { get; set; }
        public string? password { get; set; }
        public string? phone { get; set; }
        public string? bot_whatsapp { get; set; }
        public DateTimeOffset created_at { get; set; }
    }
}
