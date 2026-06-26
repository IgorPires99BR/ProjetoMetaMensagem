using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IWebhookConfigRepository
    {
        Task<Guid> Incluir(WebhookConfig webhookConfig);
        Task<Guid> Alterar(WebhookConfig webhookConfig);
        Task Excluir(Guid id);
        Task<WebhookConfig?> ObterPorId(Guid id);
        Task<List<WebhookConfig>> ObterPorEmpresa(Guid empresaId);
        Task<List<WebhookConfig>> ObterAtivosPorEvento(string evento, Guid empresaId);
        Task<List<WebhookConfig>> Obter();
    }
}
