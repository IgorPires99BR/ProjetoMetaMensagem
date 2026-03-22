using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Meta.EnviarTemplate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using ProjetoMetaMensagem.Dominio.Entidades.Meta;

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId = "956946084171393";
        private readonly string _accessToken = "EAANhG9OKJisBRI7pIeRxGYRZB5HP7ZCCFQvAhhLJsZB4nWfoid7t0EfjrFFi7k0PrU4NvZCcQ7fgVUtLWMnc9M3tGcO0BEVpsaAj4ZAzFqOZAGeuYgJcAlNrnJ2KqJNuDOYWqi05JZC3qKduOZBZA0bEd8sE7qZChWyrZCtvqxupxALC78Bq0zGU0YsFjUn6ZBdsyNRf7qBpRzLMtFUWVZCAmNFYXSD0lypUGMb0mMK9JNbzKzr2b3OWoGBzDCLgj9WqPaD7ngZCTEFcK67RXxf4mUy57TFuQU0e2z3Lp2aFIZD";

        public MetaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Ajustado para v19.0 como no seu Python
            _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v19.0/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<bool> EnviarTemplateAsync(string celular, string nomeTemplate)
        {
            // 1. Monta o objeto exatamente como o Python
            var payload = new MetaMessageRequest
            {
                To = celular,
                Template = new TemplateRequest
                {
                    Name = "hello_world",
                    Language = new LanguageRequest { Code = "en_US" }
                }
            };

            // 2. Serializa e envia
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // A URL final será: BaseAddress + {phoneNumberId}/messages
            var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EnviarTextoLivreAsync(string celular, string mensagem)
        {
            // 1. Monta o objeto seguindo a estrutura exata da Meta para texto livre
            var payload = new MetaTextMessageRequest
            {
                To = celular,
                Text = new TextContent
                {
                    PreviewUrl = true,
                    Body = mensagem
                }
            };

            // 2. Serializa (usando as configurações do Newtonsoft para respeitar os nomes da Meta)
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 3. Envia para o endpoint de mensagens
            var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Log ou tratamento de erro
                throw new Exception($"Erro na API da Meta: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}
