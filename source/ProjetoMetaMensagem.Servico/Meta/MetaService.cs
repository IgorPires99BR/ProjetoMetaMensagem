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

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _phoneNumberId = "956946084171393";
        private readonly string _accessToken = "EAAL7kEUNnu0BQ7E3oS0333DNnsgYofMwp84JSAKz7doHD7kkkR53GeuYVGfVrC7KAYfMHLaRNDq8HUNTlZBp1y3aoHzmdQULA7hcUsxWlcATZCtfXdMNCEmCePToIyf7IOktm9UBv0R5XxGECDQ95ZBDKlnfoMatLgz3TVzMtBaOLkd24q6dmSB8xu3HoAGVLzyHpmAgZAZCEymtAAu1toNPyVLZCGyZBr0ERlY3oAaTn29RQVZCFjwVge82d3kVFZBnug4Wt4GULytrKTwOTaYIOAReu1T4qViYun40ZD";

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
                Console.WriteLine($"Erro na API: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
    }
}
