using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Twilio
{
    public class TwilioService : IWhatsappService
    {

        public TwilioService()
        {
           
        }

        public async Task<bool> EnviarMensagemAsync(string numero, string mensagem)
        {
            throw new NotImplementedException();
        }
    }
}
