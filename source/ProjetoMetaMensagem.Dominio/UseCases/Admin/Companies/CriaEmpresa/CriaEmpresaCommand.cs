using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa
{
    public class CriaEmpresaCommand : IRequest<Response<CriaEmpresaResult>>
    {
        [JsonIgnore]
        public int Id { get; set; } // Referente ao company_id
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string BotWhatsapp { get; set; }
    }
}
