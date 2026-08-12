using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Webhook
{
    public class WebhookDispatcherService : IWebhookDispatcherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookDispatcherService> _logger;

        public WebhookDispatcherService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, ILogger<WebhookDispatcherService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Disparar(string evento, object payload, Guid empresaId)
        {
            var webhooks = await _unitOfWork.WebhookConfig.ObterAtivosPorEvento(evento, empresaId);

            if (webhooks == null || webhooks.Count == 0)
                return;

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            foreach (var webhook in webhooks)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();

                    if (!string.IsNullOrEmpty(webhook.TokenSegredo))
                    {
                        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhook.TokenSegredo));
                        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
                        var signature = Convert.ToBase64String(hash);
                        client.DefaultRequestHeaders.Add("X-Hub-Signature-256", $"sha256={signature}");
                    }

                    var response = await client.PostAsync(webhook.Url, content);

                    // Falha na entrega para o endpoint do tenant nao pode derrubar o processamento
                    // da mensagem, mas tambem nao pode sumir: sem log nao havia como diagnosticar
                    // webhook configurado que nunca chega no destino.
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Webhook {Nome} ({Url}) retornou {StatusCode} para o evento {Evento}.",
                            webhook.Nome, webhook.Url, (int)response.StatusCode, evento);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao disparar webhook {Nome} ({Url}) para o evento {Evento}.",
                        webhook.Nome, webhook.Url, evento);
                }
            }
        }
    }
}
