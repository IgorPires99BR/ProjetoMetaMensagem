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
        public string Provider { get; set; }
        public string Host { get; set; }
        [ConfigurationKeyName("Access_token")]
        public string Access_token { get; set; }
        public string SessionName { get; set; }
    }
}
