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
using ProjetoMetaMensagem.Servico.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiWhatsappConnectionConfiguration _configuration;

        public MetaService(HttpClient httpClient, IOptions<ApiWhatsappConnectionConfiguration> options)
        {
            _httpClient = httpClient;
            _configuration = options.Value;

            // Ajustado para v19.0 como no seu Python
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration.AccessToken);
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

            var response = await _httpClient.PostAsync($"{_configuration.PhoneNumberId}/messages", content);

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
            var response = await _httpClient.PostAsync($"{_configuration.PhoneNumberId}/messages", content);

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
