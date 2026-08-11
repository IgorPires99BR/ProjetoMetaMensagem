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

        public WebhookDispatcherService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
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

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log falha (opcional: registrar em tabela de log de disparo)
                        // _logger.LogWarning("Webhook {Nome} retornou {StatusCode}", webhook.Nome, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    // Log erro (opcional)
                    // _logger.LogError(ex, "Erro ao disparar webhook {Nome}", webhook.Nome);
                }
            }
        }
    }
}
