using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa;
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

        public Companies(AlteraEmpresaCommand command)
        {
            name = command.Nome;
            email = command.Email;
            bot_whatsapp = command.BotWhatsapp;
            phone = command.Telefone;
            created_at = DateTime.Now;
        }

        public Companies(CriaEmpresaCommand command)
        {
            name = command.Nome;
            email = command.Email;
            bot_whatsapp = command.BotWhatsapp;
            phone = command.Telefone;
            created_at = DateTime.Now;
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
