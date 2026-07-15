namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IGeminiService
    {
        Task<string> SugerirResposta(string mensagemCliente, string? contexto);
        Task<string> GerarRespostaFlow(string mensagemCliente, string variaveis, string? instrucoes);
    }
}
