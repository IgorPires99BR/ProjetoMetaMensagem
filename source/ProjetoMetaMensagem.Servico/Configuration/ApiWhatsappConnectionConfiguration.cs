using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Configuration
{
    public class ApiWhatsappConnectionConfiguration
    {
        public string BaseUrl { get; set; }
        public string AccessToken { get; set; }
        public string PhoneNumberId { get; set; }
        public string WabaID { get; set; }
    }
}
