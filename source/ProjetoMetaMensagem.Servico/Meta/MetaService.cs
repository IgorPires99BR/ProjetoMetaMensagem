using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Meta.EnviarTemplate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId = "SEU_ID";
        private readonly string _accessToken = "SEU_TOKEN";

        public MetaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v21.0/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<bool> EnviarTemplateAsync(string celular, string nomeTemplate)
        {
            var payload = new MetaMessageRequest
            {
                To = celular,
                Template = new TemplateRequest { Name = nomeTemplate }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

            return response.IsSuccessStatusCode;
        }
    }
}
