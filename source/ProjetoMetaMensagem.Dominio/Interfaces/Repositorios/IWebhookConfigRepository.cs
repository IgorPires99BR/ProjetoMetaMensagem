using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IWebhookConfigRepository
    {
        Task<Guid> Incluir(WebhookConfig webhookConfig);
        // empresaIdSolicitante restringe a operacao aos webhooks da empresa informada.
        // null = administrador (sem restricao). WebhookConfig tem EmpresaId proprio.
        Task<int> Alterar(WebhookConfig webhookConfig, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<WebhookConfig?> ObterPorId(Guid id);
        Task<List<WebhookConfig>> ObterPorEmpresa(Guid empresaId);
        Task<List<WebhookConfig>> ObterAtivosPorEvento(string evento, Guid empresaId);
        Task<List<WebhookConfig>> Obter();
    }
}
