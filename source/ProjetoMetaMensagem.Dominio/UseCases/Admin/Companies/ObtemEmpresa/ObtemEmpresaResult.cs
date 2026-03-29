using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa
{
    public class ObtemEmpresaResult
    {
        public ObtemEmpresaResult(Dominio.Entidades.Companies company)
        {
            Id = company.id;
            Name = company.name;
            Email = company.email;
            Phone = company.phone;
            BotWhatsapp = company.bot_whatsapp;
            CreatedAt = company.created_at.DateTime;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BotWhatsapp { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
