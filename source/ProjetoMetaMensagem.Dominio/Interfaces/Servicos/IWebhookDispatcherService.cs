namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IWebhookDispatcherService
    {
        Task Disparar(string evento, object payload, Guid empresaId);
    }
}
