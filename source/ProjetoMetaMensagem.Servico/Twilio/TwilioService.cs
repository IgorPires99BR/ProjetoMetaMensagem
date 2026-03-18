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
        private readonly HttpClient _httpClient;
        private readonly ApiWhatsappConnectionConfiguration _settings;

        public TwilioService(
            HttpClient httpClient,
            IOptions<ApiWhatsappConnectionConfiguration> options)
        {
            _settings = options.Value;
            _httpClient = httpClient;

            // A Twilio usa Basic Auth: Base64(AccountSid:AuthToken)
            var authToken = Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v19.0");
        }

        public async Task<bool> EnviarMensagemAsync(string numero, string mensagem)
        {
            // O endpoint da Twilio para mensagens requer o AccountSid na URL
            var url = $"{_settings.AccountSid}/Messages.json";

            // A Twilio espera x-www-form-urlencoded, não JSON
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", numero),
                new KeyValuePair<string, string>("From", _settings.FromNumber),
                new KeyValuePair<string, string>("Body", mensagem)
            });

            var response = await _httpClient.PostAsync(url, content);

            return response.IsSuccessStatusCode;
        }
    }
}
