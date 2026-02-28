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

namespace ProjetoMetaMensagem.Servico.WPPConnect
{
    public class WPPConnectService : IWhatsappService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token = "SEU_TOKEN_AQUI";

        public WPPConnectService(
            HttpClient httpClient,
            IOptions<ApiWhatsappConnectionConfiguration> options)
        {
            var settings = options.Value;

            httpClient.BaseAddress = new Uri(settings.Host);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", settings.Access_token);

            _httpClient = httpClient;
        }   

        public async Task<bool> EnviarMensagemAsync(string numero, string mensagem)
        {
            var payload = new
            {
                phone = numero,
                message = mensagem
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("session-name/send-message", content);

            return response.IsSuccessStatusCode;
        }
    }
}
