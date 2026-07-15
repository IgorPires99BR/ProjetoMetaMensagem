using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System.Text;
using System.Text.Json;

namespace ProjetoMetaMensagem.Servico.IA
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiConfiguration _config;

        public GeminiService(HttpClient httpClient, IOptions<GeminiConfiguration> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }

        public async Task<string> SugerirResposta(string mensagemCliente, string? contexto)
        {
            var prompt = $@"Você é um assistente de atendimento da ContactSolution, uma plataforma de WhatsApp para empresas.
Sugira uma resposta profissional e personalizada para a mensagem abaixo de um cliente.

{(string.IsNullOrEmpty(contexto) ? "" : $"Contexto da conversa:\n{contexto}\n")}
Mensagem do cliente: ""{mensagemCliente}""

Responda APENAS com o texto da sugestão, sem explicações adicionais, sem cumprimentos iniciais como ""Claro!"" ou ""Com certeza!"". Seja direto e objetivo.";

            return await ChamarGemini(prompt);
        }

        public async Task<string> GerarRespostaFlow(string mensagemCliente, string variaveis, string? instrucoes)
        {
            var prompt = $@"Você é um assistente virtual de uma empresa. Responda a mensagem do cliente de acordo com as instruções.

Instruções: {(string.IsNullOrEmpty(instrucoes) ? "Seja educado e objetivo, ajudando o cliente com sua solicitação." : instrucoes)}

Dados coletados até agora: {(string.IsNullOrEmpty(variaveis) || variaveis == "{{}}" ? "Nenhum dado coletado ainda." : variaveis)}

Mensagem do cliente: ""{mensagemCliente}""

Responda APENAS com o texto da resposta, sem explicações adicionais.";

            return await ChamarGemini(prompt);
        }

        private async Task<string> ChamarGemini(string prompt)
        {
            if (string.IsNullOrEmpty(_config.ApiKey))
                return "IA não configurada. Adicione a chave da API Gemini nas configurações.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_config.ApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 300
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "[Erro ao gerar sugestão]";

                using var doc = JsonDocument.Parse(jsonResponse);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text?.Trim() ?? "[Resposta vazia]";
            }
            catch
            {
                return "[Erro de conexão com a IA]";
            }
        }
    }

    public class GeminiConfiguration
    {
        public string ApiKey { get; set; }
    }
}
