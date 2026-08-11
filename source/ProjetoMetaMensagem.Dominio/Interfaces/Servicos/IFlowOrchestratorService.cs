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
        // phoneNumberIdOrigem = o phone_number_id da Meta que recebeu a mensagem (metadata do
        // webhook) -- usado pra responder pelo MESMO numero que o cliente escreveu, em vez do
        // numero "padrao" da empresa. numeroId = o Numero correspondente, ja resolvido (se
        // encontrado), reservado para telas/planos futuros que escopam Flows por numero.
        Task<FlowOrchestrationResult> ProcessarMensagem(Guid empresaId, Guid contatoId, string celular, string mensagem, string? phoneNumberIdOrigem = null, Guid? numeroId = null);
    }
}
