namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IGeminiService
    {
        Task<string> SugerirResposta(string mensagemCliente, string? contexto);
        Task<string> GerarRespostaFlow(string mensagemCliente, string variaveis, string? instrucoes);

        // Ponto de entrada generico do assistente de IA usado por Chats, Templates, Flows
        // e Disparador -- quantidade=1 tambem serve pra pedir uma unica resposta/explicacao.
        Task<List<string>> SugerirAlternativas(string instrucao, string? contexto, int quantidade = 3);
    }
}
