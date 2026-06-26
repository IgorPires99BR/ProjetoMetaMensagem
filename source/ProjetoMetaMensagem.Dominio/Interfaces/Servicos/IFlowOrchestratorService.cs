namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public class FlowOrchestrationResult
    {
        public bool Sucesso { get; set; }
        public string? Mensagem { get; set; }
        public Guid? FlowId { get; set; }
        public Guid? EtapaId { get; set; }
        public bool FlowFinalizado { get; set; }
    }

    public interface IFlowOrchestratorService
    {
        Task<FlowOrchestrationResult> ProcessarMensagem(Guid empresaId, Guid contatoId, string mensagem);
    }
}
