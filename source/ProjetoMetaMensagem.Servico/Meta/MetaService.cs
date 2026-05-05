using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using ProjetoMetaMensagem.Dominio.Entidades.Meta;
using ProjetoMetaMensagem.Servico.Configuration;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Servico.Meta.EnviarMensagem;
using ProjetoMetaMensagem.Servico.Meta.CriarTemplate;
using ProjetoMetaMensagem.Dominio.Entidades.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades;

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

        public async Task<bool> CadastrarNumeroAsync(Numero numero)
        {
            var endpoint = $"{numero.InstanciaId}/register";

            var payload = new
            {
                messaging_product = "whatsapp",
                pin = "123456" // TODO: Idealmente, este PIN deveria vir de uma configuração ou do objeto 'numero'
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao registrar número na Meta: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Tratamento de erro conforme o padrão do seu projeto
                throw new Exception($"Falha na comunicação com a Meta: {ex.Message}");
            }
        }

        public async Task<bool> EnviarTemplateAsync(string celular, string nomeTemplate)
        {
            // 1. Monta o objeto exatamente como o Python
            var payload = new MetaMessageRequest
            {
                To = celular,
                Type = "template",
                Template = new TemplateRequest
                {
                    Name = nomeTemplate, // Agora usa o que vem do Swagger
                    Language = new LanguageRequest { Code = "pt_BR" } // Ajuste conforme sua necessidade
                }
            };

            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

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

        public async Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate)
        {
            // A criação de templates é feita no endpoint do WABA_ID (WhatsApp Business Account)
            // Se o seu PhoneNumberId for o mesmo que o WABA, pode manter, 
            // caso contrário, adicione o WabaId na sua ApiWhatsappConnectionConfiguration.
            var endpoint = $"{_configuration.PhoneNumberId}/message_templates";



            var json = JsonConvert.SerializeObject(novoTemplate, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore // Ignora campos nulos como 'format' no Body
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao criar template na Meta: {responseContent}");
            }

            // Retorna o ID do template criado ou o JSON de sucesso
            return responseContent;
        }
    }
}
