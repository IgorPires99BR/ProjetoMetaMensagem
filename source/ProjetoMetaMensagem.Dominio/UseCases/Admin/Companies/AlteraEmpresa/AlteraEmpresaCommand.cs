using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa
{
    public class AlteraEmpresaCommand : IRequest<Response<AlteraEmpresaResult>>
    {
        [JsonIgnore]
        public int Id { get; set; } // Referente ao company_id
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string BotWhatsapp { get; set; }

        //(data.name, data.email, data.phone, data.bot_whatsapp, company_id)
    }
}
